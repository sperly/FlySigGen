#include <i2c_t3.h>
#include <Encoder.h>
#include <DogLcd.h>
#include <Bounce2.h>
#include "si5351.h"

// General defines
#define USB_BUFFER_SIZE			 64
#define USB_SEND_BUFFER          0b01
#define USB_RECV_BUFFER          0b10
#define USB_BOTH_BUFFER          0b11

// Knob pin Defines
#define PIN_KNOB_1_A             5
#define PIN_KNOB_1_B             6
#define PIN_KNOB_1_BUTTON        7
#define PIN_KNOB_2_A             8
#define PIN_KNOB_2_B             9
#define PIN_KNOB_2_BUTTON        10
#define PIN_KNOB_3_A             11
#define PIN_KNOB_3_B             12
#define PIN_KNOB_3_BUTTON        13

// LCD pin defines
#define PIN_LCD_SI               14
#define PIN_LCD_CLK              15
#define PIN_LCD_RS               16
#define PIN_LCD_CSB              17
#define PIN_LCD_BACKLIGHT        18

// Measurement pin defines
#define PIN_FREQ_COUNT			 3
#define PIN_ADC_0				 A2   /* ADC0_SE8 */
#define PIN_ADC_1				 A3   /* ADC1_SE9 */
#define PIN_DAC					 A14

// Instantiations
Encoder knob1(PIN_KNOB_1_A, PIN_KNOB_1_B);
Encoder knob2(PIN_KNOB_2_A, PIN_KNOB_2_B);
Encoder knob3(PIN_KNOB_3_A, PIN_KNOB_3_B);

Bounce button1 = Bounce(); 
Bounce button2 = Bounce(); 
Bounce button3 = Bounce();

//SI, CLK, RS, CSB, RESET=-1, backLight
DogLcd lcd(PIN_LCD_SI, PIN_LCD_CLK, PIN_LCD_RS, 
           PIN_LCD_CSB, -1, PIN_LCD_BACKLIGHT);

Si5351 si5351;

// Structs
typedef struct
{
  long genValue1;
  long genValue2;
  long genValue3;
}genSettings_t;

typedef enum
{
  STATE_NONE = 0,
  STATE_PRESSING,
  STATE_PRESSED,
  STATE_LONG_PRESSED,
  STATE_RELEASED
}buttonState_e;

typedef struct
{
  buttonState_e state;
  int pressTime;
  long pos;
}knobState_t;

// Variables
genSettings_t genSettings;

knobState_t knobState1;
knobState_t knobState2;
knobState_t knobState3;

uint8_t usbSendBuffer[USB_BUFFER_SIZE];
uint8_t usbRecvBuffer[USB_BUFFER_SIZE];

bool localChanged = true;

void setup()
{
  lcd.begin(DOG_LCD_M163);
  lcd.setContrast(0x28);
  lcd.setBacklight(0x128,true);
  lcd.clear();
  lcd.home();
  
  lcd.print("** FlySigGen ** ");
  lcd.setCursor(0,2);
  lcd.print("  Version 1.0   ");
  
  //Buttons setup
  pinMode(PIN_KNOB_1_BUTTON,INPUT_PULLUP);
  button1.attach(PIN_KNOB_1_BUTTON);
  button1.interval(5);
  
  pinMode(PIN_KNOB_2_BUTTON,INPUT_PULLUP);
  button2.attach(PIN_KNOB_2_BUTTON);
  button2.interval(5);
  
  pinMode(PIN_KNOB_3_BUTTON,INPUT_PULLUP);
  button3.attach(PIN_KNOB_3_BUTTON);
  button3.interval(5);
  
  si5351.init(SI5351_CRYSTAL_LOAD_8PF);
  
  knobState1.pos = -999;
  knobState2.pos = -999;
  knobState3.pos = -999;
  
  Serial.begin(9600);
  ClearUSBBuffers(USB_BOTH_BUFFER);
  
  //sleep(2);
  lcd.clear();
}

void loop()
{
  long newPosKnob1, newPosKnob2, newPosKnob3;
 
  newPosKnob1 = knob1.read();
  newPosKnob2 = knob2.read();
  newPosKnob3 = knob3.read();
 
  button1.update();
  button2.update();
  button3.update();
  
  int button1State = button1.read();
  int button2State = button1.read();
  int button3State = button1.read();
  
  if (newPosKnob1 != knobState1.pos)
  {
    
  }
  if (newPosKnob2 != knobState2.pos)
  {
    
  }
  if (newPosKnob3 != knobState3.pos)
  {
    
  }
  
  if(localChanged == true)
  {
	sendUSBData();
  }
  
  uint8_t bytesAvailable = readUSBData();
  
}

uint8_t readUSBData()
{
  int readBytes = 0;
  while (Serial.available() && readBytes <= USB_RECV_BUFFER) {
    usbRecvBuffer[readBytes++] = Serial.read();
  }
  return readBytes - 1;
}

void sendUSBData()
{
  ClearUSBBuffers(USB_SEND_BUFFER);
  
  int sendBytes = 4;
  
  // Magic bytes
  usbSendBuffer[0] = '1';
  usbSendBuffer[1] = '3';
  usbSendBuffer[2] = '3';
  usbSendBuffer[3] = '7';
  
  // Gen1 setting, 8 byte
  //usbSendBuffer[4] = 
  
  Serial.write(usbSendBuffer, sendBytes);
  //int sentBytes = RawHID.send(usbSendBuffer, sendBytes);
	
}

void RedrawMain()
{
  lcd.clear();
  printGenVal(1, genSettings.genValue1);
  printGenVal(2, genSettings.genValue2);
  printGenVal(3, genSettings.genValue3);
}

void printGenVal(int genNum, long val)
{
  lcd.setCursor(0,genNum-1);
  lcd.print(genNum);
  lcd.print(": ");
  if(val > 999999)
  {
    lcd.print(val/1000000, DEC);
    lcd.print(" MHz");
  }
  else if(val > 9999)
  {
    lcd.print(val/1000, DEC);
    lcd.print(" kHz");
  }
  else
  {
    lcd.print(val, DEC);
    lcd.print(" Hz");
  } 
}

void ClearUSBBuffers(uint8_t buffer)
{
  if(buffer & USB_SEND_BUFFER == USB_SEND_BUFFER)
  {
	memset(usbSendBuffer, 0, USB_BUFFER_SIZE);
	/*for(int i = 0;i < USB_BUFFER_SIZE;i++)
	{
	  usbSendBuffer[i] = 0;
	}*/
  }
  if(buffer & USB_RECV_BUFFER == USB_RECV_BUFFER)
  {
	memset(usbRecvBuffer, 0, USB_BUFFER_SIZE);
    /*for(int i = 0;i < USB_BUFFER_SIZE;i++)
    {
	  usbRecvBuffer[i] = 0;
    }*/
  }
}
