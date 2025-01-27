#include "BatAnalog.h"

#include "dspinst.h"
#include "sqrt_integer.h"
#include <EEPROM.h>
#include "AdcHandler.h"
#include "Helpers.h"

void BatAnalog::process()
{
	noInterrupts();
	int16_t* readyBuffer = AdcHandler::readyBuffer;
	uint8_t* powerBuffer = AdcHandler::powerReadyBuffer;
	uint16_t powerBufferSize = AdcHandler::powerReadyCount;
	interrupts();

	if (readyBuffer == nullptr)
	{
		CheckLog();
		if (_msSinceLastInfoLog >= TB_INFO_LOG_INTERVAL_MS)
		{
			AddInfoLog();
		}
		return;
	}

	// Sample duration measurement
	_lastSampleDuration = _usSinceLastSample;
	_usSinceLastSample = 0;
    
    // Save Call Duration here to prevent adding the time for FFT and other calculations to the call length
	uint32_t tmpCallDuration = _callDuration;

	// Calculate average Power of the last sample period; only use the last n samples...
	uint8_t i = 0;
	uint16_t avgPower = 0;
	if (powerBufferSize > TB_AVG_POWER_COUNT)
	{
		i = powerBufferSize - TB_AVG_POWER_COUNT;
	}
	for (; i < powerBufferSize; i++)
	{
		avgPower += powerBuffer[i];
	}
	avgPower = avgPower / TB_AVG_POWER_COUNT;

	// Check if the sample can be discarded
	BatCall* currentCall = &_callLog[_currentCallIndex];
	if (avgPower < TB_MIN_CALL_START_POWER && currentCall->sampleCount == 0)
	{
		// Power too low for a new call and no call in progress
		// Reset and exit
		noInterrupts();
		AdcHandler::MissedSamples = 0;
		AdcHandler::ClippedSignalCount = 0;
		AdcHandler::readyBuffer = nullptr;
		interrupts();
		return;
	}

	uint32_t* binData = currentCall->data;

	// Reset Data structures if new call
	if (currentCall->sampleCount == 0)
	{
		_callDuration = _lastSampleDuration;
        tmpCallDuration = _lastSampleDuration;
	    currentCall->startTimeMs = millis();
		if (Helpers::CheckLedsEnabled())
		{
			digitalWriteFast(TB_PIN_LED_GREEN, HIGH);
		}

		//Clear buffer
		memset(binData, 0, sizeof(int16_t) * TB_HALF_FFT_SIZE);
	}


	// FFT
	copy_to_fft_buffer(_complexBuffer, readyBuffer);
	apply_window_to_fft_buffer(_complexBuffer);
	arm_cfft_radix4_q15(&_fft_inst, _complexBuffer);


	// Add FFT of the last sample to the call data.
	ApplyFftSample(binData);

	currentCall->AddPowerData(powerBuffer, powerBufferSize);
	currentCall->sampleCount++;
	_msSinceLastCall = 0;


	// Check if call has ended; only use the last five samples...
	if (avgPower < TB_MIN_CALL_POWER)
	{
		currentCall->durationMicros = tmpCallDuration;

		// Take sqrt of the FFT Samples.
		for (int i = 0; i < TB_HALF_FFT_SIZE; i++)
		{
			binData[i] = (uint16_t)sqrt_uint32_approx(binData[i] / currentCall->sampleCount);
		}

		// Add sample info
		noInterrupts();
		currentCall->clippedSamples = AdcHandler::ClippedSignalCount;
		currentCall->missedSamples = AdcHandler::MissedSamples;
		interrupts();

#ifdef TB_DEBUG
		if (currentCall->missedSamples > 0)
		{
			Serial.print(F("Missed Samples: "));
			Serial.println(currentCall->missedSamples);
		}
		if (currentCall->clippedSamples > 0)
		{
			Serial.print(F("Clipped Samples: "));
			Serial.println(currentCall->clippedSamples);
		}
		Serial.printf(F("Call end Detected (Initial P: %hhu, D: %u us, End P: %hhu (%u).\n"), currentCall->sampleCount, (uint32_t)tmpCallDuration, avgPower, currentCall->powerDataLength);
#endif 
#ifdef TB_DISPLAY
		//PrintPowerData(currentCall->powerData, currentCall->powerDataLength);
		PrintSpectrum(currentCall->data);
#endif

		if (Helpers::LedsEnabled)
		{
			digitalWriteFast(TB_PIN_LED_GREEN, LOW);
		}

		noInterrupts();
		AdcHandler::MissedSamples = 0;
		AdcHandler::ClippedSignalCount = 0;
		interrupts();


		_currentCallIndex++;
		_msSinceLastCall = 0;
		CheckLog();
		_callLog[_currentCallIndex].Clear();
	}

	noInterrupts();
	AdcHandler::readyBuffer = nullptr;
	interrupts();
}

void BatAnalog::ApplyFftSample(uint32_t* binData)
{
	// The amplitude of a frequency bin is sqrt(re^2 + img^2)
	// multiply_16tx16t_add_16bx16b multiplies the lower and upper halfword of the parameters
	// and adds them together.

	// Do the DC-Offset (0 Hz) first. 
	uint32_t tmp = *((uint32_t *)_complexBuffer);
	binData[0] += multiply_16tx16t_add_16bx16b(tmp, tmp);

	int index = 2;
	for (int i = 0; i < TB_QUART_FFT_SIZE; i++)
	{
		// We are merging two bins together to get half the result set
		tmp = *((uint32_t *)_complexBuffer + index++);
		binData[i] += multiply_16tx16t_add_16bx16b(tmp, tmp) / 2;
		tmp = *((uint32_t *)_complexBuffer + index++);
		binData[i] += multiply_16tx16t_add_16bx16b(tmp, tmp) / 2;
	}
}

void BatAnalog::AddInfoLog()
{
	BatInfo* batInfo = &_infoLog[_currentInfoIndex];
	batInfo->time = Teensy3Clock.get();
	batInfo->startTimeMs = millis();
	batInfo->BatteryVoltage = AdcHandler::ReadRawBatteryVoltage();
	batInfo->LastBufferDuration = _lastSampleDuration;
#ifdef TB_DEBUG
	Serial.printf("Adding Info: Bat: %u mV, Sample Duration: %u ms, Time: %lu, MS: %lu\n", batInfo->BatteryVoltage, batInfo->LastBufferDuration, batInfo->time, batInfo->startTimeMs);
#endif
	_currentInfoIndex++;
	if (_currentInfoIndex > TB_LOG_BUFFER_LENGTH)
	{
		CheckLog();
	}
	_msSinceLastInfoLog = 0;
}

void BatAnalog::CheckLog()
{
	uint8_t switch1 = digitalReadFast(TB_PIN_S1);
	if (((_currentCallIndex > 0 || _currentInfoIndex > 0) && (switch1 == LOW || _msSinceLastCall >= TB_TIME_BEFORE_AUTO_LOG_MS)) ||
		_currentCallIndex >= TB_LOG_BUFFER_LENGTH || 
		_currentInfoIndex >= TB_LOG_BUFFER_LENGTH)
	{
		_log.LogCalls(_callLog, _currentCallIndex, _infoLog, _currentInfoIndex);
		_currentInfoIndex = 0;
		_currentCallIndex = 0;
		_callLog[0].Clear();
		while (switch1 == LOW)
		{
			switch1 = digitalReadFast(TB_PIN_S1);
		}
	}
}

void BatAnalog::copy_to_fft_buffer(void* destination, const void* source)
{
	// The FFT Buffer (dst) must contain the real sample plus a 0 for the imaginary part.
	// Hence it must be twice the size of the sample buffer.
	const uint16_t* src = (const uint16_t *)source;
	uint32_t* dst = (uint32_t *)destination;

	for (int i = 0; i < TB_FFT_SIZE; i++)
	{
		*dst++ = *src++;
	}
}

void BatAnalog::apply_window_to_fft_buffer(void* buffer)
{
	int16_t* buf = (int16_t *)buffer;
	const int16_t* win = (int16_t *)AudioWindowHanning1024;

	for (int i = 0; i < TB_FFT_SIZE; i++)
	{
		int32_t val = *buf * *win++;
		*buf = val >> 15;
		buf += 2;
	}
}


void BatAnalog::init()
{
	_nodeId = EEPROM.read(0);
	_log.SetNodeId(_nodeId);
	AdcHandler::InitAdc();
	arm_cfft_radix4_init_q15(&_fft_inst, TB_FFT_SIZE, 0, 1);
#ifdef TB_DEBUG
	Serial.printf(F("Initialization completed (NodeId: %hhu).\n"), _nodeId);
#endif
}

void BatAnalog::start()
{
	_currentCallIndex = 0;
	_callLog[_currentCallIndex].sampleCount = 0;
	AdcHandler::Start();
}

void BatAnalog::stop()
{
	AdcHandler::Stop();
}
