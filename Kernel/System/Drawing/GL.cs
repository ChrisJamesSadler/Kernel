﻿using Cosmos.Assembler;
using Cosmos.IL2CPU.Plugs;
using Kernel.System.Core;
using Math = System.Math;

namespace Kernel.System.Drawing
{
    public static unsafe class GL
    {
        public const byte fontSize = 10;
        public static uint UpdatePointer = 0;
        private static byte[] fontData = new byte[]
        {
       0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xC0,0x80,0x07,0x3F,0xFE,0xD9,0x26,0x13,0x0C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x20,0x00,0x00,0x00,0x28,0xA0,0x80,0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x09,0x04,0xFC,0x40,0x01,0x04,0x3F,0x08,0xA0,0x00,0x00,0x00,0x70,0x20,0x82,0x00,0x0C,0xC0,0x00,0x82,0x08,0x1C,0x00,0x00,0x00,0x23,0x42,0x08,0xC1,0x02,0x60,0x50,0x00,0x81,0x18,0x00,0x00,0x70,0x40,0x02,0x05,0x08,0x58,0x21,0x87,0x08,0x5C,0x00,0x00,0x80,0x00,0x02,0x08,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x40,0x00,0x00,0x02,0x08,0x20,0x80,0x00,0x00,0x10,0x00,0x80,0x00,0x04,0x10,0x00,0x00,0x02,0x08,0x00,0x40,0x00,0x01,0x00,0x10,0xE0,0x00,0x01,0x0A,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x20,0x80,0x80,0x0F,0x08,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x0E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x00,0x04,0x10,0x40,0x80,0x00,0x02,0x08,0x00,0x00,0x00,0x00,0x70,0x20,0x80,0x08,0x22,0x88,0x20,0x82,0x04,0x1C,0x00,0x00,0x00,0x00,0x0C,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x00,0x00,0x70,0x20,0x01,0x00,0x10,0x20,0x40,0x80,0x00,0x1E,0x00,0x00,0x00,0x07,0x02,0x40,0x80,0x01,0x08,0x20,0x88,0xC0,0x01,0x00,0x00,0x40,0x80,0x01,0x04,0x14,0x48,0xE0,0x03,0x04,0x10,0x00,0x00,0x00,0x0F,0x00,0x08,0xE0,0x81,0x08,0x20,0x88,0xC0,0x01,0x00,0x00,0x70,0x20,0x82,0x00,0x1E,0x88,0x20,0x82,0x08,0x1C,0x00,0x00,0x80,0x0F,0x10,0x40,0x80,0x00,0x02,0x00,0x10,0x40,0x00,0x00,0x00,0x70,0x20,0x82,0x00,0x0C,0x48,0x20,0x80,0x04,0x1C,0x00,0x00,0x00,0x07,0x22,0x88,0x20,0x02,0x0B,0x20,0x48,0xC0,0x01,0x00,0x00,0x00,0x00,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0x08,0x00,0x00,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x08,0x1C,0x08,0xC0,0x01,0x08,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xE0,0x03,0x00,0x3E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x80,0x00,0x1C,0x80,0xC0,0x81,0x00,0x00,0x00,0x00,0x00,0x07,0x22,0x80,0x00,0x01,0x02,0x08,0x00,0x80,0x00,0x00,0x00,0xC0,0x83,0x30,0x9D,0x92,0x28,0xA2,0x88,0x92,0xB4,0x31,0x08,0x00,0x02,0x18,0x00,0x40,0x02,0x09,0x7E,0x08,0x11,0x00,0x00,0x00,0xF8,0x20,0x84,0x00,0x3E,0x08,0x21,0x84,0x10,0x3E,0x00,0x00,0x00,0x0E,0x44,0x08,0x20,0x80,0x00,0x82,0x10,0x81,0x03,0x00,0x00,0xF8,0x20,0x84,0x00,0x82,0x08,0x22,0x80,0x10,0x3E,0x00,0x00,0x80,0x1F,0x02,0x08,0xE0,0x87,0x00,0x02,0x08,0xE0,0x07,0x00,0x00,0xF8,0x21,0x80,0x00,0x3E,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x00,0x1E,0x84,0x08,0x22,0x80,0x38,0x82,0x10,0x82,0x07,0x00,0x00,0x08,0x21,0x84,0x10,0x7E,0x08,0x21,0x84,0x10,0x42,0x00,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x00,0x00,0x00,0x40,0x00,0x01,0x04,0x10,0x40,0x20,0x81,0x04,0x0C,0x00,0x00,0x80,0x10,0x22,0x48,0xE0,0x80,0x05,0x22,0x88,0x21,0x04,0x00,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x3E,0x00,0x00,0x80,0x61,0x86,0x09,0xA5,0x94,0x42,0x22,0xC9,0x24,0x11,0x00,0x00,0x18,0x61,0x84,0x12,0x42,0x48,0x21,0x86,0x18,0x42,0x00,0x00,0x00,0x1E,0xC4,0x08,0x22,0x88,0x20,0x82,0x10,0x83,0x03,0x00,0x00,0xF8,0x20,0x84,0x10,0x42,0xF8,0x20,0x80,0x00,0x02,0x00,0x00,0x00,0x0E,0xC4,0x08,0x22,0x88,0x20,0x82,0x90,0x81,0x0B,0x00,0x00,0xF8,0x21,0x84,0x10,0x7E,0x88,0x20,0x86,0x10,0x82,0x00,0x00,0x00,0x0F,0x42,0x08,0xC0,0x01,0x18,0x40,0x08,0xC1,0x03,0x00,0x00,0xF8,0x81,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x00,0x00,0x80,0x10,0x42,0x08,0x21,0x84,0x10,0x42,0x10,0xC1,0x03,0x00,0x00,0x08,0x21,0x84,0x10,0x24,0x90,0x00,0x00,0x06,0x18,0x00,0x00,0x00,0x18,0x62,0x48,0x09,0x21,0xA4,0x84,0x30,0x86,0x10,0x00,0x00,0x08,0x41,0x02,0x04,0x08,0x60,0x40,0x02,0x00,0x42,0x00,0x00,0x80,0x10,0x02,0x90,0x00,0x01,0x02,0x08,0x20,0x80,0x00,0x00,0x00,0xF8,0x01,0x02,0x04,0x00,0x20,0x40,0x80,0x00,0x7E,0x00,0x00,0x80,0x01,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x00,0x00,0x20,0x80,0x00,0x02,0x00,0x40,0x00,0x01,0x00,0x00,0x00,0x80,0x01,0x04,0x10,0x40,0x00,0x01,0x04,0x10,0x40,0x00,0x01,0x00,0x10,0xC0,0x80,0x02,0x12,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x08,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x70,0x20,0x01,0x07,0x12,0x48,0xE0,0x02,0x00,0x00,0x08,0x20,0x80,0x07,0x26,0x88,0x20,0x82,0x09,0x1E,0x00,0x00,0x00,0x00,0x00,0x70,0x20,0x82,0x00,0x02,0x88,0xC0,0x01,0x00,0x00,0x80,0x00,0x02,0x0F,0x24,0x88,0x20,0x02,0x09,0x3C,0x00,0x00,0x00,0x00,0x00,0x70,0x00,0x82,0x0F,0x02,0x90,0xC0,0x01,0x00,0x00,0x30,0x20,0x80,0x01,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0xF0,0x40,0x82,0x08,0x22,0x90,0xC0,0x03,0x08,0x00,0x08,0x20,0x80,0x06,0x26,0x88,0x20,0x82,0x08,0x22,0x00,0x00,0x80,0x00,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x00,0x00,0x00,0x08,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x00,0x80,0x00,0x02,0x48,0xA0,0x80,0x01,0x0A,0x48,0x20,0x00,0x00,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0x68,0x63,0x8B,0x24,0x92,0x48,0x22,0x09,0x00,0x00,0x00,0x00,0x80,0x0F,0x26,0x88,0x20,0x82,0x08,0x22,0x00,0x00,0x00,0x00,0x00,0x70,0x40,0x82,0x08,0x22,0x90,0xC0,0x01,0x00,0x00,0x00,0x00,0x80,0x07,0x26,0x88,0x20,0x82,0x09,0x1E,0x08,0x00,0x00,0x00,0x00,0xF0,0x40,0x82,0x08,0x22,0x90,0xC0,0x03,0x08,0x00,0x00,0x00,0x80,0x03,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0x70,0x20,0x80,0x01,0x18,0x40,0xE0,0x01,0x00,0x00,0x08,0x20,0x80,0x01,0x02,0x08,0x20,0x80,0x00,0x06,0x00,0x00,0x00,0x00,0x00,0x88,0x20,0x82,0x08,0x22,0xC8,0xC0,0x02,0x00,0x00,0x00,0x00,0x80,0x00,0x12,0x48,0x40,0x00,0x03,0x08,0x00,0x00,0x00,0x00,0x00,0x40,0xA2,0x89,0x02,0x6A,0x90,0x41,0x02,0x00,0x00,0x00,0x00,0x80,0x04,0x04,0x30,0xC0,0x80,0x04,0x32,0x00,0x00,0x00,0x00,0x00,0x88,0x20,0x01,0x04,0x04,0x30,0x80,0x00,0x01,0x00,0x00,0x00,0x80,0x07,0x10,0x20,0x40,0x80,0x00,0x1E,0x00,0x00,0x00,0x03,0x04,0x10,0x40,0x00,0x00,0x00,0x10,0x40,0x00,0x01,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x00,0x80,0x00,0x04,0x10,0x40,0x00,0x02,0x04,0x10,0x40,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x0E,0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x7C,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x1F,0x00,0x00,0xC0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x10,0x00,0x00,0x01,0x04,0x10,0x40,0x00,0x01,0x00,0x40,0x00,0x01,0x07,0x10,0x28,0x20,0x00,0x09,0x1C,0x10,0x00,0x00,0x07,0x22,0x08,0x20,0x80,0x03,0x04,0x38,0x00,0x03,0x00,0x00,0x00,0x20,0x82,0x07,0x02,0x08,0xE0,0x81,0x08,0x00,0x00,0x00,0x00,0x08,0x02,0x50,0xC0,0x80,0x0F,0x08,0xF8,0x80,0x00,0x00,0x00,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x80,0x00,0x02,0x08,0x00,0x00,0x07,0x12,0x10,0xE0,0x80,0x0C,0x26,0x60,0x00,0x81,0x04,0x00,0x28,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x0F,0x42,0xE0,0x52,0x40,0x09,0xB8,0x08,0xC1,0x03,0x00,0x00,0x38,0x80,0x80,0x03,0x0E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x05,0x0A,0x50,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x3E,0x80,0x00,0x02,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xF0,0x20,0x04,0x2E,0x21,0xE4,0x00,0x8A,0x10,0x3C,0x00,0xF0,0x07,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x38,0x20,0x80,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x08,0x20,0xE0,0x03,0x02,0x08,0x00,0xE0,0x03,0x00,0x00,0x38,0x80,0x00,0x01,0x0E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x80,0x03,0x04,0x20,0x60,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x10,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x08,0x20,0x80,0x00,0x12,0x48,0xE0,0x81,0x00,0x00,0xF8,0x70,0xC2,0x09,0x26,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x06,0x10,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x18,0xA0,0x80,0x02,0x06,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xA0,0x00,0x05,0x28,0x50,0xA0,0x00,0x00,0x00,0x10,0x62,0x04,0x11,0x24,0x40,0x84,0x18,0x72,0x04,0x01,0x00,0x00,0x21,0x46,0x10,0x41,0x02,0x74,0x08,0x21,0x42,0x1C,0x00,0x00,0x38,0x42,0x00,0x12,0x26,0x40,0x04,0x18,0x72,0x04,0x01,0x00,0x00,0x00,0x00,0x20,0x00,0x00,0x02,0x08,0x10,0x20,0x80,0x08,0x00,0x20,0x80,0x01,0x00,0x24,0x90,0xE0,0x87,0x10,0x01,0x00,0x00,0x00,0x02,0x18,0x00,0x40,0x02,0x09,0x7E,0x08,0x11,0x00,0x00,0x00,0x20,0x80,0x01,0x00,0x24,0x90,0xE0,0x87,0x10,0x01,0x00,0x00,0x00,0x02,0x18,0x00,0x40,0x02,0x09,0x7E,0x08,0x11,0x00,0x00,0x00,0x20,0x80,0x01,0x00,0x24,0x90,0xE0,0x87,0x10,0x01,0x00,0x00,0x00,0x06,0x18,0x00,0x40,0x02,0x09,0x7E,0x08,0x11,0x00,0x00,0x00,0xC0,0x8F,0x02,0x0A,0xE4,0x93,0xC0,0x83,0x08,0xE0,0x03,0x00,0x00,0x0E,0x44,0x08,0x20,0x80,0x00,0x82,0x10,0x81,0x03,0x00,0x00,0xF8,0x21,0x80,0x00,0x7E,0x08,0x20,0x80,0x00,0x7E,0x00,0x00,0x80,0x1F,0x02,0x08,0xE0,0x87,0x00,0x02,0x08,0xE0,0x07,0x00,0x00,0xF8,0x21,0x80,0x00,0x7E,0x08,0x20,0x80,0x00,0x7E,0x00,0x00,0x80,0x1F,0x02,0x08,0xE0,0x87,0x00,0x02,0x08,0xE0,0x07,0x00,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x00,0x00,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x00,0x00,0x00,0xF8,0x20,0x84,0x10,0x0F,0x08,0x20,0x84,0x10,0x3E,0x00,0x00,0x80,0x11,0x46,0x28,0x21,0x84,0x14,0x62,0x88,0x21,0x04,0x00,0x00,0xE0,0x41,0x8C,0x20,0x82,0x08,0x22,0x08,0x31,0x38,0x00,0x00,0x00,0x1E,0xC4,0x08,0x22,0x88,0x20,0x82,0x10,0x83,0x03,0x00,0x00,0xE0,0x41,0x8C,0x20,0x82,0x08,0x22,0x08,0x31,0x38,0x00,0x00,0x00,0x1E,0xC4,0x08,0x22,0x88,0x20,0x82,0x10,0x83,0x03,0x00,0x00,0xE0,0x41,0x8C,0x20,0x82,0x08,0x22,0x08,0x31,0x38,0x00,0x00,0x00,0x00,0x00,0x88,0xC0,0x01,0x02,0x14,0x88,0x00,0x00,0x00,0x00,0xE0,0x42,0x8C,0x20,0xA2,0x48,0xA2,0x08,0x31,0x7E,0x00,0x00,0x80,0x10,0x42,0x08,0x21,0x84,0x10,0x42,0x10,0xC1,0x03,0x00,0x00,0x08,0x21,0x84,0x10,0x42,0x08,0x21,0x04,0x11,0x3C,0x00,0x00,0x80,0x10,0x42,0x08,0x21,0x84,0x10,0x42,0x10,0xC1,0x03,0x00,0x00,0x08,0x21,0x84,0x10,0x42,0x08,0x21,0x04,0x11,0x3C,0x00,0x00,0x80,0x10,0x02,0x90,0x00,0x01,0x02,0x08,0x20,0x80,0x00,0x00,0x00,0x08,0x20,0x80,0x0F,0x42,0x08,0xE1,0x83,0x00,0x02,0x00,0x00,0x00,0x07,0x02,0x48,0x20,0x81,0x04,0x02,0x28,0xA1,0x03,0x00,0x04,0x20,0x00,0x00,0x07,0x12,0x70,0x20,0x81,0x04,0x2E,0x00,0x80,0x00,0x02,0x00,0x70,0x20,0x01,0x07,0x12,0x48,0xE0,0x02,0x00,0x08,0x50,0x00,0x00,0x07,0x12,0x70,0x20,0x81,0x04,0x2E,0x00,0x40,0x00,0x04,0x00,0x70,0x20,0x01,0x07,0x12,0x48,0xE0,0x02,0x00,0x00,0x50,0x00,0x00,0x07,0x12,0x70,0x20,0x81,0x04,0x2E,0x00,0x00,0x00,0x06,0x00,0x70,0x20,0x01,0x07,0x12,0x48,0xE0,0x02,0x00,0x00,0x00,0x00,0x00,0x7F,0x22,0xF2,0x2F,0x82,0x8C,0xDC,0x01,0x00,0x00,0x00,0x00,0x70,0x20,0x82,0x00,0x02,0x88,0xC0,0x01,0x02,0x04,0x20,0x00,0x00,0x07,0x20,0xF8,0x20,0x00,0x09,0x1C,0x00,0x80,0x00,0x02,0x00,0x70,0x00,0x82,0x0F,0x02,0x90,0xC0,0x01,0x00,0x18,0x40,0x00,0x00,0x07,0x20,0xF8,0x20,0x00,0x09,0x1C,0x00,0x00,0x00,0x05,0x00,0x70,0x00,0x82,0x0F,0x02,0x90,0xC0,0x01,0x00,0x01,0x08,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x40,0x00,0x01,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x00,0x00,0x06,0x18,0x00,0x80,0x00,0x02,0x08,0x20,0x80,0x00,0x02,0x00,0x00,0x40,0x01,0x00,0x08,0x20,0x80,0x00,0x02,0x08,0x20,0x00,0x00,0x00,0x70,0xC0,0x00,0x04,0x1C,0x48,0x20,0x80,0x04,0x0C,0x00,0x40,0x00,0x04,0x00,0xF8,0x60,0x82,0x08,0x22,0x88,0x20,0x02,0x00,0x04,0x20,0x00,0x00,0x07,0x24,0x88,0x20,0x02,0x09,0x1C,0x00,0x80,0x00,0x02,0x00,0x70,0x40,0x82,0x08,0x22,0x90,0xC0,0x01,0x00,0x18,0x60,0x00,0x00,0x07,0x24,0x88,0x20,0x02,0x09,0x1C,0x00,0x40,0x00,0x04,0x00,0x70,0x40,0x82,0x08,0x22,0x90,0xC0,0x01,0x00,0x00,0x28,0x00,0x00,0x07,0x24,0x88,0x20,0x02,0x09,0x1C,0x00,0x00,0x00,0x00,0x00,0x20,0x00,0x80,0x0F,0x00,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x0F,0x26,0xC8,0xA0,0x82,0x09,0x1E,0x00,0x40,0x00,0x02,0x00,0x88,0x20,0x82,0x08,0x22,0xC8,0xC0,0x02,0x00,0x08,0x20,0x00,0x80,0x08,0x22,0x88,0x20,0x82,0x0C,0x2C,0x00,0x80,0x00,0x05,0x00,0x88,0x20,0x82,0x08,0x22,0xC8,0xC0,0x02,0x00,0x00,0x50,0x00,0x80,0x08,0x22,0x88,0x20,0x82,0x0C,0x2C,0x00,0x80,0x00,0x02,0x00,0x88,0x20,0x01,0x04,0x04,0x30,0x80,0x00,0x01,0x00,0x08,0x20,0x80,0x07,0x26,0x88,0x20,0x82,0x09,0x1E,0x08,0x00,0x80,0x02,0x00,0x88,0x20,0x01,0x04,0x04,0x30,0x80,0x00,0x01
        };

        public static void Update()
        {
            if (UpdatePointer == 0)
            {
                Utils.memcpy(Settings.DisplayVRam, Settings.DisplayBuffer, Settings.DisplayWidth * Settings.DisplayHeight);
            }
            else
            {
                Caller.CallCode(UpdatePointer, null);
            }
        }

        public static void DrawText(uint* Buffer, int bufferWidth, int X, int Y, string msg, uint Colour)
        {
            for (int c = 0; c < msg.Length; c++)
            {
                DrawCharacter(Buffer, bufferWidth, X, Y, msg[c], Colour);
                X += fontSize;
            }
        }

        private static int NumberDepth(int value, int depth = 0)
        {
            if (value < 0)
            {
                depth = NumberDepth(-value, depth);
                depth++;
            }
            else
            {
                long n = value / 10;
                long r = value % 10;
                if (r < 0)
                {
                    r += 10;
                    n--;
                }
                if (value >= 10)
                {
                    depth = NumberDepth((int)n, depth);
                    depth++;
                }
            }
            return depth;
        }

        public static void DrawNumber(uint* Buffer, int bufferWidth, int X, int Y, int value, uint Colour)
        {
            int depth = NumberDepth(value);
            if (value < 0)
            {
                DrawCharacter(Buffer, bufferWidth, X, Y, '-', Colour);
                DrawNumber(Buffer, bufferWidth, X, Y, -value, Colour);
            }
            else
            {
                long n = value / 10;
                long r = value % 10;
                if (r < 0)
                {
                    r += 10;
                    n--;
                }
                if (value >= 10)
                {
                    DrawNumber(Buffer, bufferWidth, X, Y, (int)n, Colour);
                }
                X += (depth * fontSize);
                DrawCharacter(Buffer, bufferWidth, X, Y, Console.nchars[(int)r], Colour);
            }
        }

        public static void DrawCharacter(uint* Buffer, int bufferWidth, int X, int Y, char aChar, uint Colour)
        {
            int pointer = aChar * (fontSize * fontSize);
            int currentBit = 0;
            for (int y = 0; y < fontSize; y++)
            {
                for (int x = 0; x < fontSize; x++)
                {
                    if (IsSet(pointer + currentBit))
                    {
                        SetPixel(Buffer, bufferWidth, X + x, Y + y, Colour);
                    }
                    currentBit++;
                }
            }
        }

        private static bool IsSet(int entry)
        {
            return (fontData[entry / 8] & (1 << (entry % 8))) != 0;
        }

        [PlugMethod(PlugRequired = true)]
        public static void Clear(uint* dst, uint value, int len) { }

        [PlugMethod(PlugRequired = true)]
        public static void DrawRectangleFilled(uint* buffer, int bufferWidth, uint value, int X, int Y, int Width, int Height) { }

        public static void DrawRectangleEmpty(uint* buffer, int bufferWidth, uint Colour, int X, int Y, int Width, int Height)
        {
            DrawLine(buffer, bufferWidth, X, Y, X + Width, Y, Colour);
            DrawLine(buffer, bufferWidth, X, Y, X, Y + Height, Colour);
            DrawLine(buffer, bufferWidth, X, Y + Height, X + Width, Y + Height, Colour);
            DrawLine(buffer, bufferWidth, X + Width, Y, X + Width, Y + Height, Colour);
        }

        public static void DrawLine(uint* buffer, int bufferWidth, int x0, int y0, int x1, int y1, uint c)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap(ref x0, ref y0); Swap(ref x1, ref y1); }
            if (x0 > x1) { Swap(ref x0, ref x1); Swap(ref y0, ref y1); }
            int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

            for (int x = x0; x <= x1; ++x)
            {
                if (steep)
                {
                    SetPixel(buffer, bufferWidth, y, x, c);
                }
                else
                {
                    SetPixel(buffer, bufferWidth, x, y, c);
                }
                err = err - dY;
                if (err < 0) { y += ystep; err += dX; }
            }
        }

        private static void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        public static void SetPixel(uint* Buffer, int bufferWidth, int X, int Y, uint Colour)
        {
            uint* where = Buffer + (Y * bufferWidth) + X;
            where[0] = Colour;
        }

        public static void DrawImage(uint* destination, int destWidth, int destHeight, uint* source, int sourWidth, int sourHeight, int x, int y, bool transparent = false)
        {
            if(transparent)
            {
                // some asm plug here later
            }
            else
            {
                destination = destination + (destWidth * y) + x;
                for(int Y0 = 0; Y0 < sourHeight; Y0++)
                {
                    Utils.memcpy(destination, source, sourWidth);
                    destination += destWidth;
                }
            }
        }

        [Plug(Target = typeof(GL))]
        public class GLPlug
        {
            [PlugMethod(Assembler = typeof(asmGLClear))]
            public static void Clear(uint* dst, uint value, int len) { }

            public class asmGLClear : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("mov eax, [ebp+12]");
                    new LiteralAssemblerCode("mov edi, [ebp+16]");
                    new LiteralAssemblerCode("cld");
                    new LiteralAssemblerCode("mov ecx, [ebp+8]");
                    new LiteralAssemblerCode("rep stosd");
                }
            }

            [PlugMethod(Assembler = typeof(asmGLRectangleFilled))]
            public static void DrawRectangleFilled(uint* buffer, int bufferWidth, uint value, int X, int Y, int Width, int Height) { }

            public class asmGLRectangleFilled : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    // set size of buffer
                    new LiteralAssemblerCode("mov eax, [ebp+28]");
                    new LiteralAssemblerCode("mov ecx, 4");
                    new LiteralAssemblerCode("mul ecx");
                    new LiteralAssemblerCode("mov [ebp+28], eax");

                    // set x
                    new LiteralAssemblerCode("mov eax, [ebp+20]");
                    new LiteralAssemblerCode("mov ecx, 4");
                    new LiteralAssemblerCode("mul ecx");
                    new LiteralAssemblerCode("mov [ebp+20], eax");

                    // calculate the start point
                    new LiteralAssemblerCode("mov ecx, [ebp+16]");
                    new LiteralAssemblerCode("mov eax, [ebp+28]");
                    new LiteralAssemblerCode("mul ecx");
                    new LiteralAssemblerCode("mov ecx, [ebp+32]");
                    new LiteralAssemblerCode("add eax, ecx");
                    new LiteralAssemblerCode("mov [ebp+32], eax");

                    new LiteralAssemblerCode("makeLine:");
                    new LiteralAssemblerCode("mov eax, [ebp+32]");
                    new LiteralAssemblerCode("mov ecx, [ebp+20]");
                    new LiteralAssemblerCode("add eax, ecx");
                    new LiteralAssemblerCode("mov edi, eax");
                    new LiteralAssemblerCode("mov eax, [ebp+24]");
                    new LiteralAssemblerCode("cld");
                    new LiteralAssemblerCode("mov ecx, [ebp+12]");
                    new LiteralAssemblerCode("rep stosd");
                    new LiteralAssemblerCode("mov eax, [ebp+8]");
                    new LiteralAssemblerCode("sub eax, 1");
                    new LiteralAssemblerCode("mov [ebp+8], eax");
                    new LiteralAssemblerCode("cmp eax, 0");
                    new LiteralAssemblerCode("je doneLine");
                    new LiteralAssemblerCode("mov eax, [ebp+32]");
                    new LiteralAssemblerCode("mov ecx, [ebp+28]");
                    new LiteralAssemblerCode("add eax, ecx");
                    new LiteralAssemblerCode("mov [ebp+32], eax");
                    new LiteralAssemblerCode("jmp makeLine");
                    new LiteralAssemblerCode("doneLine:");
                }
            }
        }
    }
}