from enum import Enum

from BaseClasses import ItemClassification

class DHItemType(Enum):
	GAME_MODE = 0
	OBJECT = 1
	ABILITY = 2
	RECIPE = 3
	POWERUP = 4
	SPACE = 10
	PROGRESSIVE = 11
	FILLER = 50
	COSMETIC_HEAD = 100
	COSMETIC_BODY = 101
	COSMETIC_TRAIL = 102

class DHItem:
	id: int
	name: str
	sub_name: str
	type: DHItemType
	count: int
	ap_class: ItemClassification

	def __init__(self, type_: DHItemType, data, count: int = 1):
		if isinstance(data, tuple):
			sub_id, sub_name = data
		else:
			sub_id = data.value
			sub_name = data.name()

		self.type = type_
		self.id = (type_.value << 8) | sub_id
		self.name = self.sub_name = sub_name
		self.count = count

		match type_:
			case DHItemType.GAME_MODE:
				self.name = f"\"{sub_name}\" game mode"
				self.ap_class = ItemClassification.progression

			case DHItemType.OBJECT:
				self.ap_class = ItemClassification.progression

			case DHItemType.ABILITY:
				self.ap_class = ItemClassification.progression if sub_id < 10 else ItemClassification.useful

			case DHItemType.RECIPE:
				self.name += " recipe"
				self.ap_class = ItemClassification.progression

			case DHItemType.POWERUP:
				self.ap_class = ItemClassification.progression

			case DHItemType.SPACE:
				self.ap_class = ItemClassification.useful

			case DHItemType.PROGRESSIVE:
				self.ap_class = ItemClassification.useful
				if sub_id < 100:
					self.name = f'starting "{sub_name}" level'
					self.ap_class = ItemClassification.filler

			case DHItemType.FILLER:
				self.ap_class = ItemClassification.filler

			case DHItemType.COSMETIC_HEAD:
				self.ap_class = ItemClassification.filler

			case DHItemType.COSMETIC_BODY:
				self.ap_class = ItemClassification.filler

			case DHItemType.COSMETIC_TRAIL:
				self.ap_class = ItemClassification.filler
