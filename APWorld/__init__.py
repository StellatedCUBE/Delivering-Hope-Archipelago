from typing import List
import random

from BaseClasses import Region, Tutorial, ItemClassification, Item, Location, LocationProgressType
from Options import OptionError
from worlds.AutoWorld import WebWorld, World

from .data import *
from .regions import DHRegion
from .objects import DHObject
from .abilities import DHAbility
from .powerups import DHPowerUp
from .options import DHOptions, dh_option_groups
from .location import DHLocationType

class DHWebWorld(WebWorld):
	setup_en = Tutorial(
		tutorial_name = "Multiworld Setup Guide",
		description = "A guide to playing Delivering Hope with Archipelago.",
		language = "English",
		file_name = "setup_en.md",
		link = "setup/en",
		authors = ["StellatedCUBE"]
	)

	tutorials = [setup_en]
	game_info_languages = ["en"]
	option_groups = dh_option_groups
	rich_text_options_doc = True

class DHAPItem(Item):
	game = GAME

class DHAPLocation(Location):
	game = GAME

class DHWorld(World):
	"""Delivering Hope is a game about throwing IRyS as far as you can while not letting her get distracted by Bae figurines."""

	game = GAME
	web = DHWebWorld()
	options: DHOptions
	options_dataclass = DHOptions
	location_name_to_id = dh_location_name_to_id
	item_name_to_id = dh_item_name_to_id
	origin_region_name = DHRegion.MENU.name()
	filler_items: List[str]
	
	def generate_early(self) -> None:
		self.filler_items = [
			item.name
			for item in dh_items
			if item.type == DHItemType.FILLER
		]
		self.cosmetic_pool = self.create_cosmetic_item_pool()
		self.made_cosmetic_filler = False

		self.distances = {
			DHRegion.MENU: max(self.options.goal_standard.value, self.options.goal_classic.value),
			DHRegion.BAELESS: self.options.goal_baeless.value,
			DHRegion.GACHA: self.options.goal_gacha.value
		}

		for location in dh_locations:
			if location.type == DHLocationType.ACHIEVEMENT and location.sub_name in self.options.goal_achievements:
				self.distances[location.region] = max(self.distances.get(location.region, 0), location.distance)

	def create_item(self, name: str) -> DHAPItem:
		return DHAPItem(name, dh_item_name_to_object[name].ap_class, dh_item_name_to_object[name].id, self.player)
	
	def create_primary_item_pool(self) -> List[str]:
		return [
			item.name
			for item in dh_items
			if item.type.value < 10 or (item.type == DHItemType.SPACE and (item.id & 255) > 2)
		]
	
	def create_cosmetic_item_pool(self) -> List[str]:
		return [
			item.name
			for item in dh_items
			if item.type.value >= 100
		]
	
	def create_items(self) -> None:
		self.create_locations()
		locations = self.location_names

		item_pool = self.create_primary_item_pool()

		for item in dh_items:
			if item.type == DHItemType.PROGRESSIVE and item.sub_name in self.options.powerup_starting_levels:
				for _ in range(item.count):
					item_pool.append(item.name)

		starting_boosters = [
			DHObject.MORI.name(),
			DHObject.MUMEI.name(),
			DHObject.KRONII.name()
		]

		precollected = {
			item.name
			for item in self.multiworld.precollected_items[self.player]
		}

		precollected_new = set()

		starting_booster_count = sum(booster in precollected for booster in starting_boosters)

		if starting_booster_count == 0:
			starting_booster = self.random.choice(starting_boosters)
			precollected.add(starting_booster)
			precollected_new.add(starting_booster)

		if starting_booster_count < 2:
			if DHObject.MORI.name() in precollected:
				self.early(self.random.choice(starting_boosters[1:]))
			else:
				self.early(DHObject.MORI)

		self.early(DHAbility.DOWNBOOST)

		if DHObject.STRAWBERRY.name() in precollected:
			item_pool.append('double strawberry')
		else:
			item_pool.remove(DHObject.STRAWBERRY.name())
			item_pool.extend(['progressive strawberry'] * 2)

		for powerup in DHPowerUp:
			if not powerup.has_additional_behaviour() and powerup.name() not in self.options.powerup_items:
				precollected_new.add(powerup.name())

		if DHPowerUp.HOT_SAUCE.name() in self.options.powerup_items and DHPowerUp.VERY_HOT_SAUCE.name() in self.options.powerup_items:
			item_pool.remove(DHPowerUp.HOT_SAUCE.name())
			item_pool.remove(DHPowerUp.VERY_HOT_SAUCE.name())
			item_pool.extend(['progressive hot sauce'] * 2)
		else:
			for powerup in (DHPowerUp.HOT_SAUCE, DHPowerUp.VERY_HOT_SAUCE):
				if powerup.name() not in self.options.powerup_items:
					precollected_new.add(powerup.name())

		if DHPowerUp.LAST_HOPE.name() in self.options.powerup_items:
			item_pool.remove(DHPowerUp.LAST_HOPE.name())
			item_pool.extend(['progressive last hope'] * 2)
		else:
			precollected_new.add(DHPowerUp.LAST_HOPE.name())
			item_pool.append('super last hope')

		if DHPowerUp.DIVORCE_PAPERS.name() in self.options.powerup_items:
			item_pool.remove(DHPowerUp.DIVORCE_PAPERS.name())
			item_pool.extend(['progressive divorce papers'] * 2)
		else:
			precollected_new.add(DHPowerUp.DIVORCE_PAPERS.name())
			item_pool.append('super divorce papers')
		
		item_pool = [
			item
			for item in item_pool
			if item not in precollected_new
		]

		for item in precollected_new:
			self.push_precollected(self.create_item(item))

		needed = len(locations) - len(item_pool)
		if needed > 0:
			for _ in range(min(needed * self.options.cosmetic_items.value // 100, len(self.cosmetic_pool))):
				self.made_cosmetic_filler = True
				item_pool.append(self.cosmetic_pool.pop(self.random.randrange(len(self.cosmetic_pool))))

		self.remove_items(item_pool, len(locations), ItemClassification.filler)
		self.remove_items(item_pool, len(locations), ItemClassification.useful)

		if len(item_pool) > len(locations):
			raise OptionError("Not enough checks are enabled")
		
		for item in item_pool:
			self.multiworld.itempool.append(self.create_item(item))

		for _ in range(len(item_pool), len(locations)):
			self.multiworld.itempool.append(self.create_item(self.random.choice(self.filler_items)))


	def remove_items(self, pool: List[str], max_size: int, class_: ItemClassification) -> None:
		while len(pool) > max_size:
			removable = [
				i
				for i, item_name in enumerate(pool)
				if dh_item_name_to_object[item_name].ap_class == class_
			]

			if not removable:
				return

			del pool[self.random.choice(removable)]

	def early(self, item: Enum | str) -> None:
		name = item if isinstance(item, str) else item.name()
		if not any(pci.name == name for pci in self.multiworld.precollected_items[self.player]):
			self.multiworld.early_items[self.player][name] = 1

	def create_regions(self) -> None:
		menu_region = None

		for region in DHRegion:
			new_region = Region(region.name(), self.player, self.multiworld)
			self.multiworld.regions.append(new_region)

			if menu_region is None:
				menu_region = new_region
			else:
				item = [item for item in dh_items if item.type == DHItemType.GAME_MODE and item.sub_name == region.name()]
				if item:
					def rule(state, item_name = item[0].name, player = self.player):
						return state.has(item_name, player)

					menu_region.connect(new_region, rule = rule)
	
	def create_locations(self) -> None:
		enabled = [
			location
			for location in dh_locations
			if (
				location.type != DHLocationType.ACHIEVEMENT and (
					location.type != DHLocationType.FUSION or self.options.fusion_checks.value
				)
			) or location.sub_name in self.options.achievement_checks
		]

		id_to_name = {
			item.id: item.name
			for item in dh_items
		}

		for location in enabled:
			region = self.get_region(location.region.name())
			ap_location = DHAPLocation(self.player, location.name, location.id, region)
			if location.distance > self.distances.get(location.region, 0):
				ap_location.progress_type = LocationProgressType.EXCLUDED
			elif location.priority:
				ap_location.progress_type = LocationProgressType.PRIORITY
			if location.type == DHLocationType.SHOP:
				ap_location.item_rule = lambda item: item.game != GAME or 'Hope Stone' not in item.name
			rule = location.create_rule(self.player, id_to_name)
			if rule is not None:
				ap_location.access_rule = rule
			region.locations.append(ap_location)

		self.location_names = [
			location.name
			for location in enabled
		]

	def get_filler_item_name(self) -> str:
		if self.cosmetic_pool and self.random.randrange(100) < self.cosmetic_items.value:
			self.made_cosmetic_filler = True
			return self.cosmetic_pool.pop(self.random.randrange(len(self.cosmetic_pool)))
		else:
			return self.random.choice(self.filler_items)

	def set_rules(self) -> None:
		locations = self.location_names
		p = self.player
		self.multiworld.completion_condition[self.player] = lambda state: all(state.can_reach_location(l, p) for l in locations)
	
	def fill_slot_data(self) -> Dict[str, Any]:
		game_id = ''
		for _ in range(12):
			game_id += random.choice('1234567890qwertyuiopasdfghjklzxcvbnm')

		return dict(
			gameId = game_id,
			cosmeticsAreItems = self.made_cosmetic_filler,
			deathLink = self.options.death_link.value,
			rebirthBlocksDeathLink = self.options.death_link_blockable.value,
			spaceMult = self.options.space_speed_multiplier.value / 100,
			HSMult = self.options.hope_stones_multiplier.value / 100,
			goalStandardDistance = self.options.goal_standard.value,
			goalClassicDistance = self.options.goal_classic.value,
			goalBaelessDistance = self.options.goal_baeless.value,
			goalGachaDistance = self.options.goal_gacha.value,
			goalCellTime = self.options.goal_cell.value * 100,
			goalSpaceDistance = self.options.goal_space_d.value * self.options.space_speed_multiplier.value / 1e5,
			goalSpaceScore = self.options.goal_space_s.value,
			goalAchievements = [
				location.id & 255
				for location in dh_locations
				if location.type == DHLocationType.ACHIEVEMENT and location.sub_name in self.options.goal_achievements
			]
		)
