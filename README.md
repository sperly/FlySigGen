# FlySigGen
Signal generator based on a Teensy 3 and a Si5351 breakout board.

To control the Si5351 a rotary encoder is used.
There is a COG-163 display from EA showing configuration and current setting.

The Teensy is also using USB to show/control the "signal generator".

To control the Si5351 a library from https://github.com/etherkit/Si5351Arduino is used,
and it is using the i2c_t3 library from https://forum.pjrc.com/threads/21680-New-I2C-library-for-Teensy3
The display uses the DOGLcd from PJRC.
Rotary encoders uses the Encoder library from PJRC.
To communicate via USB, the Teensy uses USB_RAW_HID

On the PC side there is a cli application for Linux to control the generation
and a Windows application to giv a GUI with both showing current settings and
controlling the settings.
