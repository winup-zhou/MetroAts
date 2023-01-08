using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuAts
{
	public class ATS_P
	{
		public static int ATSBrake;
		public static bool ATSRed;
		public static double PatternSig, PatternRed, PatternLim1, PatternLim2, PatternLim3, PatternLim4, PatternLim5;
		public static int PatternSpeed1, PatternSpeed2, PatternSpeed3, PatternSpeed4, PatternSpeed5;
		public static double PatternSpeed, ReleaseSpeed;
		public static int BrakeTimer;
		public static bool WarningLamp, BrakeLamp, EmrbrakeLamp, ATSPower;
		public static int ATSChime, PatternStart, PatternEnd, Warning, Brake;
		public static bool ATSPattern;
		public static double Release;
		public static int ATSData, ATSCut;
		public static bool ATSCutLamp;
		public static int BeaconNumber, WarningSound1, WarningSound2;
		public static bool WarningSoundX;
		public static int WarningTimer;
		public static bool WestPattern;
		public static int SignalWarning;
		public static void PInit(int Param)
		{
			ATSBrake = 0;
			ATSRed = false;
			ATSData = 0;
			ATSPattern = false;
			PatternSig = 0;
			PatternRed = 0;
			PatternLim1 = 0;
			PatternLim2 = 0;
			PatternLim3 = 0;
			PatternLim4 = 0;
			PatternLim5 = 0;
			PatternSpeed1 = 0;
			PatternSpeed2 = 0;
			PatternSpeed3 = 0;
			PatternSpeed4 = 0;
			PatternSpeed5 = 0;
			PatternSpeed = Config.PMaxspeed;
			ReleaseSpeed = Config.PMaxspeed - 5;
			BrakeTimer = 0;
			ATSCut = 0;
			ATSCutLamp = false;
			WarningLamp = false;
			BrakeLamp = false;
			if (Param == 1)
				ATSPower = true;
			else
				ATSPower = false;
		}
		public static void PStart(int time)
		{
			ATSPower = true;
			ATSRed = true;
			if (BrakeTimer == 0)
				BrakeTimer = time + 5000;
		}
		public static void PStartTimer(int time)
		{
			if (BrakeTimer > 0 && BrakeTimer < time)
			{
				ATSPower = true;
				ATSRed = false;
				ATSBrake = 0;
				BrakeTimer = 0;
			}
		}
		public static void PRun(double deltaL)
		{
			WarningSound1 = ATS_SOUND_CONTINUE;
			WarningSound2 = ATS_SOUND_CONTINUE;
			ATSChime = ATS_SOUND_CONTINUE;
			Brake = ATS_SOUND_CONTINUE;
			PatternStart = ATS_SOUND_CONTINUE;
			PatternEnd = ATS_SOUND_CONTINUE;
			Warning = ATS_SOUND_CONTINUE;
			if (PatternSig != 0)
			{
				PatternSig -= deltaL;
				if (ATSData < 1)
					ATSData = 1;
			}

			if (PatternRed > 0)
				PatternRed -= deltaL;
			else
				PatternRed = 0;
			if (PatternLim1 > -26 && PatternLim1 != 0)
				PatternLim1 -= deltaL;
			else
				PatternLim1 = 0;
			if (PatternLim2 != 0)
				PatternLim2 -= deltaL;
			if (PatternLim3 != 0)
				PatternLim3 -= deltaL;
			if (PatternLim4 != 0)
				PatternLim4 -= deltaL;
			if (PatternLim5 != 0)
				PatternLim5 -= deltaL;

			if (PatternLim1 != 0 || PatternLim2 != 0 || PatternLim3 != 0 || PatternLim4 != 0 || PatternLim5 != 0 || PatternRed != 0 || ATSData != 0)
			{
				if (!ATSPattern)
				{
					ATSChime = ATS_SOUND_PLAY;
					PatternStart = ATS_SOUND_PLAY;
				}
				ATSPattern = true;
			}
			else
			{
				if (ATSPattern)
				{
					ATSChime = ATS_SOUND_PLAY;
					PatternEnd = ATS_SOUND_PLAY;
				}
				ATSPattern = false;
			}
			PatternSpeed = g_ini.DATA.PMaxspeed;
			if (PatternSpeed > PPattern(PatternLim1 + 1, PatternSpeed1, g_ini.DATA.Pdecelerate) && PatternLim1 != 0)
				PatternSpeed = PPattern(PatternLim1 + 1, PatternSpeed1, g_ini.DATA.Pdecelerate);
			if (PatternSpeed > PPattern(PatternLim2, PatternSpeed2, g_ini.DATA.Pdecelerate) && PatternLim2 != 0)
				PatternSpeed = PPattern(PatternLim2, PatternSpeed2, g_ini.DATA.Pdecelerate);
			if (PatternSpeed > PPattern(PatternLim3, PatternSpeed3, g_ini.DATA.Pdecelerate) && PatternLim3 != 0)
				PatternSpeed = PPattern(PatternLim3, PatternSpeed3, g_ini.DATA.Pdecelerate);
			if (PatternSpeed > PPattern(PatternLim4, PatternSpeed4, g_ini.DATA.Pdecelerate) && PatternLim4 != 0)
				PatternSpeed = PPattern(PatternLim4, PatternSpeed4, g_ini.DATA.Pdecelerate);
			if (PatternSpeed > PPattern(PatternLim5, PatternSpeed5, g_ini.DATA.Pdecelerate) && PatternLim5 != 0)
				PatternSpeed = PPattern(PatternLim5, PatternSpeed5, g_ini.DATA.Pdecelerate);
			if (PatternSpeed > 15 && PatternRed > 0)
				PatternSpeed = 15;

			ReleaseSpeed = g_ini.DATA.PMaxspeed - 5;
			if (ReleaseSpeed > PPattern(PatternLim1, PatternSpeed1, g_ini.DATA.Pdecelerate) - 5 && PatternLim1 != 0)
				ReleaseSpeed = PPattern(PatternLim1, PatternSpeed1, g_ini.DATA.Pdecelerate) - 5;
			if (ReleaseSpeed > PPattern(PatternLim2, PatternSpeed2, g_ini.DATA.Pdecelerate) - 5 && PatternLim2 != 0)
				ReleaseSpeed = PPattern(PatternLim2, PatternSpeed2, g_ini.DATA.Pdecelerate) - 5;
			if (ReleaseSpeed > PPattern(PatternLim3, PatternSpeed3, g_ini.DATA.Pdecelerate) - 5 && PatternLim3 != 0)
				ReleaseSpeed = PPattern(PatternLim3, PatternSpeed3, g_ini.DATA.Pdecelerate) - 5;
			if (ReleaseSpeed > PPattern(PatternLim4, PatternSpeed4, g_ini.DATA.Pdecelerate) - 5 && PatternLim4 != 0)
				ReleaseSpeed = PPattern(PatternLim4, PatternSpeed4, g_ini.DATA.Pdecelerate) - 5;
			if (ReleaseSpeed > PPattern(PatternLim5, PatternSpeed5, g_ini.DATA.Pdecelerate) - 5 && PatternLim5 != 0)
				ReleaseSpeed = PPattern(PatternLim5, PatternSpeed5, g_ini.DATA.Pdecelerate) - 5;

			if (ATSBrake > 0 && Release > ReleaseSpeed)
				Release = ReleaseSpeed;
			else if (ATSBrake == 0)
				Release = g_ini.DATA.PMaxspeed - 5;
			if (g_ini.DATA.West == 0)
				SignalWarning = -5;
			else if (g_ini.DATA.West == 1 || g_ini.DATA.West == 2 && WestPattern)
				SignalWarning = 5;
			if (PatternRed > 0 && speed > 10
				|| g_ini.DATA.PMaxspeed - 5 < speed
				|| PWarning(PatternSig, speed, 0, SignalWarning, g_ini.DATA.Pdecelerate, g_ini.DATA.Margin)
				|| PWarning(PatternLim1, speed, PatternSpeed1, PatternSpeed1 - 5, g_ini.DATA.Pdecelerate, 0)
				|| PWarning(PatternLim2, speed, PatternSpeed2, PatternSpeed2 - 5, g_ini.DATA.Pdecelerate, 0)
				|| PWarning(PatternLim3, speed, PatternSpeed3, PatternSpeed3 - 5, g_ini.DATA.Pdecelerate, 0)
				|| PWarning(PatternLim4, speed, PatternSpeed4, PatternSpeed4 - 5, g_ini.DATA.Pdecelerate, 0)
				|| PWarning(PatternLim5, speed, PatternSpeed5, PatternSpeed5 - 5, g_ini.DATA.Pdecelerate, 0)
				)
			{
				if (!WarningLamp)
				{
					ATSChime = ATS_SOUND_PLAY;
					Warning = ATS_SOUND_PLAY;
					WarningSoundX = false;
					WarningTimer = time;
				}
				WarningLamp = true;
				if (WarningTimer <= time && !WarningSoundX && ATSBrake == 0)
				{
					WarningSound1 = ATS_SOUND_PLAY;
					WarningSoundX = true;
					WarningTimer += 4000;
				}
				else if (WarningTimer <= time && ATSBrake == 0)
				{
					WarningSound2 = ATS_SOUND_PLAY;
					WarningSoundX = false;
					WarningTimer += 4000;
				}
			}
			else
			{
				if (WarningLamp)
				{
					ATSChime = ATS_SOUND_PLAY;
					Warning = ATS_SOUND_PLAY;
				}
				WarningLamp = false;
			}
			if (ATSCut > time)
			{
				if (!ATSCutLamp)
					ATSChime = ATS_SOUND_PLAY;
				ATSCutLamp = true;
			}
			else
			{
				if (ATSCutLamp)
					ATSChime = ATS_SOUND_PLAY;
				ATSCutLamp = false;
			}
			if (ATSRed)
				ATSBrake = 2;
			else if (15 < speed && PatternRed > 0 && ATSBrake <= 1 && !ATSCutLamp)
				ATSBrake = 2;
			else if (PPatternSig(PatternSig, 10, g_ini.DATA.Pdecelerate) < speed && PPatternSig(PatternSig, 10, g_ini.DATA.Pdecelerate) > 0 && ATSBrake <= 1 && !ATSCutLamp)
			{
				if (g_ini.DATA.PEnabled > 0)
					ATSBrake = 1;
				else if (g_ini.DATA.PEnabled < 0)
					ATSBrake = 2;
				Release = 0;
			}
			else if (PatternSpeed < speed && ATSBrake < 1 && !ATSCutLamp)
			{
				if (g_ini.DATA.PEnabled > 0)
					ATSBrake = 1;
				else if (g_ini.DATA.PEnabled < 0)
					ATSBrake = 2;
			}
			else if (Release > speed && ATSBrake == 1)
			{
				ATSBrake = 0;
			}
			if (ATSCutLamp)
			{
				ATSBrake = 0;
			}
			if (ATSBrake != 0)
			{
				if (!BrakeLamp)
				{
					ATSChime = ATS_SOUND_PLAY;
					Brake = ATS_SOUND_PLAY;
					WarningSoundX = false;
					WarningTimer = time;
				}
				BrakeLamp = true;
				if (WarningTimer <= time && !WarningSoundX && speed != 0)
				{
					WarningSound1 = ATS_SOUND_PLAY;
					WarningSoundX = true;
					WarningTimer += 1500;
				}
				else if (WarningTimer <= time && speed != 0)
				{
					WarningSound2 = ATS_SOUND_PLAY;
					WarningSoundX = false;
					WarningTimer += 1500;
				}
			}
			else
			{
				if (BrakeLamp)
					ATSChime = ATS_SOUND_PLAY;
				BrakeLamp = false;
			}
			if (PatternSpeed > PPatternSig(PatternSig, 10, g_ini.DATA.Pdecelerate) && PatternSig != 0)
				PatternSpeed = PPatternSig(PatternSig, 10, g_ini.DATA.Pdecelerate);
		}
		public static double PPatternSig(double distance, int speed, double decelerate)
		{
			double PatternSpeed;

			if (distance - g_ini.DATA.Margin > 0)
			{
				PatternSpeed = Math.Sqrt(7.2 * decelerate * (distance - g_ini.DATA.Margin));
				if (speed > PatternSpeed)
					PatternSpeed = speed;
			}
			else if (distance > 0)
			{
				PatternSpeed = speed;
			}
			else
			{
				PatternSpeed = 0;
			}

			return PatternSpeed;
		}
		public static double PPattern(double distance, int speed, double decelerate)
		{
			double PatternSpeed;

			if (distance > 0)
			{
				PatternSpeed = Math.Sqrt((speed * speed) + (7.2 * decelerate * distance));
			}
			else if (distance < 0)
			{
				PatternSpeed = speed;
			}
			else
			{
				PatternSpeed = 0;
			}

			return PatternSpeed;
		}
		public static bool PWarning(double distance, double speed, int Limit, int ReleaseSpeed, double decelerate, int Margin)
		{
			bool Warning;
			double Pattern = (speed * speed - Limit * Limit) / 7.2 / decelerate + Margin;

			if (distance - 50 < Pattern && speed > ReleaseSpeed && distance != 0)
				Warning = true;
			else
				Warning = false;
			return Warning;
		}
		public static void PBeacon(int Type, int Signal, double distance, int Optional)
		{
			if (Type == 3)
			{
				if (ATSData < 1)
					ATSData = 1;
				if (Signal == 0 && distance > 0)
				{
					PatternSig = distance;
					if (Optional != 0)
						BeaconNumber = Optional;
				}
				else if (Optional == BeaconNumber && Optional != 0 && Signal != 0)
				{
					PatternSig = 0;
					BeaconNumber = Optional;
				}
				else if (PatternSig + 5 > distance && PatternSig - 5 < distance && Signal != 0)
				{
					PatternSig = 0;
					if (Optional != 0)
						BeaconNumber = Optional;
				}
				else if (Optional == 9 && PatternSig == 0)
					BeaconNumber = Optional;
				if (Optional != 0)
					WestPattern = true;
				else
					WestPattern = false;
			}
			if (Type == 4 && distance > 0)
			{
				if (Signal == 0)
				{
					PatternSig = distance;
					if (distance < 50)
						ATSBrake = 2;
				}
				if (ATSData < 1)
					ATSData = 1;
			}
			if (Type == 5 && distance > 0)
			{
				if (Signal == 0)
				{
					PatternSig = distance;
					if (distance < 50)
					{
						ATSBrake = 1;
						PatternRed = 80;
					}
				}
				if (ATSData < 1)
					ATSData = 1;
			}
			if (Type == 6 && Optional > 1000)
			{
				PatternLim1 = Optional / 1000 - 1;
				PatternSpeed1 = Optional % 1000;

			}
			if (Type == 7 && Optional > 2)
			{
				if (Optional / 1000 != 0)
					PatternLim2 = Optional / 1000;
				else
					PatternLim2 = -5;
				PatternSpeed2 = Optional % 1000;
			}
			if (Type == 8 && Optional > 1000)
			{
				if (Optional / 1000 != 0)
					PatternLim3 = Optional / 1000;
				else
					PatternLim3 = -5;
				PatternSpeed3 = Optional % 1000;
			}
			if (Type == 9 && Optional > 1000)
			{
				if (Optional / 1000 != 0)
					PatternLim4 = Optional / 1000;
				else
					PatternLim4 = -5;
				PatternSpeed4 = Optional % 1000;
			}
			if (Type == 10 && Optional > 1000)
			{
				if (Optional / 1000 != 0)
					PatternLim5 = Optional / 1000;
				else
					PatternLim5 = -5;
				PatternSpeed5 = Optional % 1000;
			}
			if (Type == 16 && Optional == 0)
			{
				PatternLim1 = 0;
				PatternSpeed1 = 0;
			}
			if (Type == 17 && Optional == 0)
			{
				PatternLim2 = 0;
				PatternSpeed2 = 0;
			}
			if (Type == 18 && Optional == 0)
			{
				PatternLim3 = 0;
				PatternSpeed3 = 0;
			}
			if (Type == 19 && Optional == 0)
			{
				PatternLim4 = 0;
				PatternSpeed4 = 0;
			}
			if (Type == 20 && Optional == 0)
			{
				PatternLim5 = 0;
				PatternSpeed5 = 0;
			}
		}
	}
	public class ATS_S
    {
		public static bool ATSPower,ATSRed,ATSBrake,ATSChime,ATSWhite,Space;
		public static int LongTimer,PsTimer,SpeedTimer0,SpeedTimer1,SpeedTimer2,SpeedTimer3,SpeedTimer4,bell,chime,chimeR,chimeL,chimeX,bellstop;
		public static bool chimeLR;
		public static void SnInit(int Param)
		{
			ATSBrake = false;
			ATSChime = false;
			ATSRed = false;
			LongTimer = 0;
			SpeedTimer0 = 0;
			SpeedTimer1 = 0;
			SpeedTimer2 = 0;
			SpeedTimer3 = 0;
			SpeedTimer4 = 0;
			chimeX = 0;
			chimeLR = false;
			chimeL = ATS_SOUND_CONTINUE;
			chimeR = ATS_SOUND_CONTINUE;
			chime = ATS_SOUND_STOP;
			if (Param == 0)
			{
				ATSPower = false;
				ATSWhite = false;
			}
			else
			{
				ATSPower = true;
				ATSWhite = true;
			}
		}
		public static void SnStart(int time)
		{
			if (!ATSPower)
			{
				ATSPower = true;
				ATSRed = true;
				ATSChime = false;
				if (LongTimer == 0)
					LongTimer = -time - 1000;
			}
		}
		public static void SnRun(int time)
		{
			if (LongTimer < 0 && -LongTimer < time)
			{
				ATSRed = false;
				ATSChime = true;
				ATSWhite = true;
				LongTimer = 0;
			}
			if (LongTimer < time - 5000 && LongTimer > 0 || EB.EBBrake)
			{
				ATSBrake = true;
				ATSRed = true;
				ATSChime = false;
				ATSWhite = false;
				LongTimer = 0;
			}
			else if (LongTimer > 0)
			{
				ATSRed = true;
				ATSWhite = false;
				ATSChime = false;
			}

			if (ATSRed)
				bell = ATS_SOUND_PLAYLOOPING;
			else
			{
				if (bell == ATS_SOUND_PLAYLOOPING)
					bellstop = ATS_SOUND_PLAY;
				else
					bellstop = ATS_SOUND_CONTINUE;
				bell = ATS_SOUND_STOP;
			}
			if (ATSChime)
			{
				chime = ATS_SOUND_PLAYLOOPING;
				if (chimeX != 0)
				{
					if (chimeX < time && !chimeLR)
					{
						chimeR = ATS_SOUND_PLAY;
						chimeX += 250;
						chimeLR = !chimeLR;
					}
					else if (chimeX < time)
					{
						chimeL = ATS_SOUND_PLAY;
						chimeX += 250;
						chimeLR = !chimeLR;
					}
					else
					{
						chimeL = ATS_SOUND_CONTINUE;
						chimeR = ATS_SOUND_CONTINUE;
					}
				}
				else
					chimeX = time;
			}
			else
			{
				chimeL = ATS_SOUND_CONTINUE;
				chimeR = ATS_SOUND_CONTINUE;
				chime = ATS_SOUND_STOP;
				chimeX = 0;
				chimeLR = false;
			}
		}
		public static void SnBeacon(int Type, int Signal, int Optional,int time)
		{
			if (Type == 0)
			{
				if (Optional == 0 && Signal == 0)
				{
					if (!ATSBrake)
						LongTimer = time;
				}
				else if (Optional != 0 && SpeedTimer4 > time)
				{
					if (!ATSBrake)
						LongTimer = time;
				}
			}
			if (Type == 1)
			{
				if (Optional == 0 && Signal == 0)
				{
					if (!ATSBrake)
						LongTimer = time - 5000;
				}
				else if (Optional == 1 && SpeedTimer4 > time && Signal == 0)
				{
					if (!ATSBrake)
						LongTimer = time - 5000;
				}
				else if (Optional == 2 && SpeedTimer4 > time)
				{
					if (!ATSBrake)
						LongTimer = time - 5000;
				}
			}
			/*if (Type == 9 && Optional > 0 && Optional < speed)
			{
				if (!ATSBrake)
					LongTimer = time - 5000;
			}
			if (Type == 12)
			{
				if (Optional < speed && Optional > 0 && Signal == 0 && Ps.Pa1 == 0 && Ps.Pb1 == 0)
				{
					if (!ATSBrake)
						LongTimer = time - 5000;
				}
				else if (Signal == 0 && Ps.Pa1 == 0 && Ps.Pb1 == 0)
				{
					if (SpeedTimer0 > time && SpeedTimer0 < time - g_ini.DATA.SnSpeedTimer)
					{
						if (!ATSBrake)
							LongTimer = time - 5000;
					}
					SpeedTimer0 = time + g_ini.DATA.SnSpeedTimer;
				}
			}
			if (Type == 50)
			{
				if (Optional == 0)
				{
					if (SpeedTimer1 > time && SpeedTimer1 < time - g_ini.DATA.SnSpeedTimer)
					{
						if (!ATSBrake)
							LongTimer = time - 5000;
					}
					SpeedTimer1 = time + g_ini.DATA.SnSpeedTimer;
				}
				else if (Signal == 0 && Ps.Pa1 == 0 && Ps.Pb1 == 0)
				{
					if (SpeedTimer1 > time && SpeedTimer1 < time - g_ini.DATA.SnSpeedTimer)
					{
						if (!ATSBrake)
							LongTimer = time - 5000;
					}
					SpeedTimer1 = time + g_ini.DATA.SnSpeedTimer;
				}
			}
			if (Type == 55)
			{
				if (Optional == 0)
				{
					if (Signal == 0 && Ps.Pa1 == 0 && Ps.Pb1 == 0)
					{
						if (SpeedTimer2 > time && SpeedTimer2 < time - g_ini.DATA.SnSpeedTimer)
						{
							if (!ATSBrake)
								LongTimer = time - 5000;
						}
						SpeedTimer2 = time + g_ini.DATA.SnSpeedTimer;
					}
				}
				else
				{
					if (ATSPower && !ATSBrake && speed > Optional)
						LongTimer = time - 5000;
				}
			}*/
			if (Type == 60)
			{
				SpeedTimer3 = time;
			}
			if (Type == 61 && Optional > 0 && Signal == 0)
			{
				if (SpeedTimer3 > time + Optional)
					LongTimer = time - 5000;
			}
			if (Type == 65)
			{
				SpeedTimer4 = time + Optional;
			}
		}
		public static void SnButton(int atsKeyCode)
		{
			if (atsKeyCode == ATS_KEY_S)
			{
				if (LongTimer > 0 && ATSNotch <= BrakeNotch && !ATSBrake)
				{
					LongTimer = 0;
					ATSRed = false;
					ATSWhite = true;
					ATSChime = true;
				}
			}
			if (atsKeyCode == ATS_KEY_A1)
			{
				if (ATSChime)
				{
					LongTimer = 0;
					ATSRed = false;
					ATSWhite = true;
					ATSChime = false;
				}
			}
			if (atsKeyCode == ATS_KEY_B1)
			{
				if (ATSBrake && BrakeNotch == emgBrake && speed == 0)
				{
					LongTimer = 0;
					ATSBrake = false;
					ATSRed = false;
					ATSWhite = true;
					ATSChime = true;
				}
			}
		}
	}
}
