﻿// 
// Teensy Bat Explorer - Copyright(C) 2020 Meinrad Jean-Richard
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;

namespace TeensyBatExplorer.Core
{
    public static class BinaryReaderHelper
    {
        public static void SkipBytes(this BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (reader.ReadByte() != 0)
                {
                    // Ups...
                }
            }
        }

        public static DateTime ReadDateTimeWithMicroseconds(this BinaryReader reader)
        {
            long unixTimestamp = reader.ReadUInt32();
            long microsOffset = reader.ReadUInt32();

            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return dateTime.AddMilliseconds(microsOffset / 1000d).DateTime;
        }
    }
}