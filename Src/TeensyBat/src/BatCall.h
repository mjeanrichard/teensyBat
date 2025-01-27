#ifndef BATCALL_h
#define BATCALL_h

#include "Config.h"

class BatCall
{
private:
public:
	uint32_t startTimeMs = 0;
	uint32_t durationMicros = 0;
	uint16_t sampleCount = 0;
	uint16_t missedSamples = 0;
	uint16_t clippedSamples = 0;
	uint16_t powerDataLength = 0;
	uint8_t powerData[TB_POWER_DATA_COUNT];
	uint32_t data[TB_HALF_FFT_SIZE] __attribute__((aligned(4)));

	void AddPowerData(uint8_t * power, uint16_t length)
	{
		for (uint16_t i = 0; i < length; i++) {
			if (powerDataLength < TB_POWER_DATA_COUNT)
			{
				powerData[powerDataLength] = power[i];
				powerDataLength++;
			}
		}
	}

	void Clear()
	{
		powerDataLength = 0;
		sampleCount = 0;
	}
};

class BatInfo
{
private:
public:
	uint32_t time = 0;
	uint32_t startTimeMs = 0;
	uint16_t BatteryVoltage = 0;
	uint16_t LastBufferDuration = 0;
};

#endif
