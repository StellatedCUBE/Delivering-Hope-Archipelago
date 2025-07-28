from enum import Enum

class DHObject(Enum):
	MORI = 3
	BANANA = 5
	JETPACK = 7
	GLIDER = 8
	MONOPOLY = 9
	KRONII = 10
	MUMEI = 11
	KIARA = 12
	INA = 15
	GURA = 16
	FAUNA = 17
	STRAWBERRY = 22
	ROCKET = 23
	SHIORI = 26
	NERISSA = 27
	BIBOO = 28
	FUWAMOCO = 29

	def name(self) -> str:
		match self:
			case DHObject.MORI: return "Mori Calliope"
			case DHObject.BANANA: return "banana"
			case DHObject.JETPACK: return "jet pack"
			case DHObject.GLIDER: return "glider"
			case DHObject.MONOPOLY: return "Holopoly"
			case DHObject.KRONII: return "Ouro Kronii"
			case DHObject.MUMEI: return "Nanashi Mumei"
			case DHObject.KIARA: return "Takanashi Kiara"
			case DHObject.INA: return "Ninomae Ina'nis"
			case DHObject.GURA: return "Gawr Gura"
			case DHObject.FAUNA: return "Ceres Fauna"
			case DHObject.STRAWBERRY: return "strawberry"
			case DHObject.ROCKET: return "supersonic rocket"
			case DHObject.SHIORI: return "Shiori Novella"
			case DHObject.NERISSA: return "Nerissa Ravencroft"
			case DHObject.BIBOO: return "Koseki Bijou"
			case DHObject.FUWAMOCO: return "FuwaMoco"
