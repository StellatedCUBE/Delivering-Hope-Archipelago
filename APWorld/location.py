from typing import List, Any, Dict, Optional
from enum import Enum

from .regions import DHRegion
from .objects import DHObject
from .abilities import DHAbility
from .recipes import DHRecipe
from .powerups import DHPowerUp
from .item import DHItemType

def item_id(type_: DHItemType, sub_id: Enum):
	return (type_.value << 8) | sub_id.value

def build_requirement(requirement: Any) -> List[int]:
	if isinstance(requirement, DHRegion):
		return [item_id(DHItemType.GAME_MODE, requirement)]
	if isinstance(requirement, DHObject):
		return [item_id(DHItemType.OBJECT, requirement)]
	if isinstance(requirement, DHAbility):
		return [item_id(DHItemType.ABILITY, requirement)]
	if isinstance(requirement, DHRecipe):
		return [item_id(DHItemType.RECIPE, requirement)]
	if isinstance(requirement, DHPowerUp):
		return [item_id(DHItemType.POWERUP, requirement)]

	if isinstance(requirement, tuple):
		or_group = []
		for subrequirement in requirement:
			or_group.extend(build_requirement(subrequirement))
		return or_group
	
	raise TypeError(f"Bad requirement type {type(requirement)}")

class DHLocationType(Enum):
	ACHIEVEMENT = 0
	SHOP = 1
	SPACE = 2
	FUSION = 3

class DHLocation:
	id: int
	name: str
	sub_name: str
	type: DHLocationType
	region: DHRegion
	default: bool
	item_requirements: List[List[int]]
	priority: bool
	
	def __init__(self, type_: DHLocationType, sub_id: int, sub_name: str, default: bool, requirements: List[Any]):
		self.type = type_
		self.id = (type_.value << 8) | sub_id
		self.sub_name = sub_name
		self.region = DHRegion.MENU
		self.default = default
		self.priority = False

		match type_:
			case DHLocationType.ACHIEVEMENT:
				self.name = f"Achievement \"{sub_name}\""
			case DHLocationType.SHOP:
				self.name = sub_name
				self.priority = sub_id > 9
			case DHLocationType.SPACE:
				self.name = sub_name
				self.priority = not(sub_id & 1)
			case DHLocationType.FUSION:
				self.name = f"Fusion \"{sub_name}\""

		self.item_requirements = []
		for requirement in requirements:
			if isinstance(requirement, DHRegion) and self.region == DHRegion.MENU:
				self.region = requirement
			elif requirement is not None:
				self.item_requirements.append(build_requirement(requirement))

	def create_rule(self, player: int, id_to_name: Dict[int, str]):
		if self.item_requirements:
			return lambda state: all(
				any(
					state.has(id_to_name[item], player)
					for item in or_group
				)
				for or_group in self.item_requirements
			)
