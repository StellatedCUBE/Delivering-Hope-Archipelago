from dataclasses import dataclass

from Options import Choice, Toggle, PerGameCommonOptions, StartInventoryPool, DeathLink, OptionGroup, OptionSet, Range, DefaultOnToggle

from .powerups import DHPowerUp
from .location import DHLocationType
from .item import DHItemType
from .data import *

class DeathLinkBlockable(Toggle):
	"""Can a “Rebirth” item be used to block an incoming death link?"""
	display_name = "Rebirth Blocks Death Link"

class AchievementChecks(OptionSet):
	"""Which achievements are checks.
	Make sure to select enough to cover all the items."""
	display_name = "Achievement Checks"
	default = {
		location.sub_name
		for location in dh_locations
		if location.type == DHLocationType.ACHIEVEMENT and location.default
	}
	valid_keys = [
		location.sub_name
		for location in dh_locations
		if location.type == DHLocationType.ACHIEVEMENT
	]

class FusionChecks(DefaultOnToggle):
	"""If creating fusions are checks."""
	display_name = "Fusions"

class PowerUpItems(OptionSet):
	"""Which power-ups need to be unlocked before they can be bought.
	Use this instead of “Start Inventory From Pool” to prevent issues with progressive item naming."""
	display_name = "Power-Up Unlocks"
	default = {
		powerup.name()
		for powerup in DHPowerUp
		if not powerup.default()
	}
	valid_keys = [
		powerup.name()
		for powerup in DHPowerUp
	]

class PowerUpStartingLevels(OptionSet):
	"""Include initial levelling for these power-ups as items."""
	display_name = "Power-Up Initial Levelling Items"
	default = {
		item.sub_name
		for item in dh_items
		if DHItemType.PROGRESSIVE.value << 8 <= item.id < (DHItemType.PROGRESSIVE.value << 8) + 100
	}
	valid_keys = [
		item.sub_name
		for item in dh_items
		if DHItemType.PROGRESSIVE.value << 8 <= item.id < (DHItemType.PROGRESSIVE.value << 8) + 100
	]

class CosmeticItems(Range):
	"""Percent of filler items that should be cosmetics.
	If set to zero, or if no filler items are generated,
	cosmetics are obtained in-game as they are normally."""
	display_name = "Cosmetic Filler (%)"
	default = 0
	range_start = 0
	range_end = 100

class SpaceSpeedMultiplier(Range):
	"""Speed up (or slow down) the “Space” game mode by reducing target scores by this factor."""
	display_name = "Space Speed Multiplier (%)"
	default = 100
	range_start = 10
	range_end = 2000

class HopeStonesMultiplier(Range):
	"""Increase (or decrease) the amount of Hope Stones granted as in-game rewards.
	Does not affect Hope Stones given over Archipelago."""
	display_name = "Hope Stone Multiplier (%)"
	default = 100
	range_start = 10
	range_end = 1000

class GoalStandardDistance(Range):
	"""The required distance to be achieved in “Standard” mode to reach the goal state."""
	display_name = "“Standard” Distance (m)"
	default = 100000
	range_start = 0
	range_end = 150000

class GoalClassicDistance(Range):
	"""The required distance to be achieved in “Classic” mode to reach the goal state."""
	display_name = "“Classic” Distance (m)"
	default = 0
	range_start = 0
	range_end = 150000

class GoalBaelessDistance(Range):
	"""The required distance to be achieved in “Baeless” mode to reach the goal state."""
	display_name = "“Baeless” Distance (m)"
	default = 3000
	range_start = 0
	range_end = 5000

class GoalGachaDistance(Range):
	"""The required distance to be achieved in “Gacha” mode to reach the goal state."""
	display_name = "“Gacha” Distance (m)"
	default = 0
	range_start = 0
	range_end = 150000

class GoalTheCellTime(Range):
	"""The required time for “The Cell” mode to be beaten in to reach the goal state.
	Set to 300 to disable this requirement."""
	display_name = "“The Cell” Time (s)"
	default = 180
	range_start = 120
	range_end = 300

class GoalSpaceDistance(Range):
	"""The required distance to be achieved in “Space” mode to reach the goal state."""
	display_name = "“Space” Distance (m)"
	default = 0
	range_start = 0
	range_end = 200000

class GoalSpaceScore(Range):
	"""The required total score to be achieved in “Space” mode to reach the goal state.
	This **is** affected by “Space Speed Multiplier”."""
	display_name = "“Space” Score"
	default = 8300
	range_start = 0
	range_end = 10000

class GoalAchievements(OptionSet):
	"""Which achievements are required to reach the goal state."""
	display_name = "Required Achievements"
	default = {}
	valid_keys = [
		location.sub_name
		for location in dh_locations
		if location.type == DHLocationType.ACHIEVEMENT
	]

@dataclass
class DHOptions(PerGameCommonOptions):
	# General
	death_link: DeathLink
	death_link_blockable: DeathLinkBlockable
	space_speed_multiplier: SpaceSpeedMultiplier
	hope_stones_multiplier: HopeStonesMultiplier
	
	# Locations
	achievement_checks: AchievementChecks
	fusion_checks: FusionChecks

	# Items
	powerup_items: PowerUpItems
	powerup_starting_levels: PowerUpStartingLevels
	cosmetic_items: CosmeticItems

	# Goal
	goal_standard: GoalStandardDistance
	goal_classic: GoalClassicDistance
	goal_baeless: GoalBaelessDistance
	goal_gacha: GoalGachaDistance
	goal_cell: GoalTheCellTime
	goal_space_d: GoalSpaceDistance
	goal_space_s: GoalSpaceScore
	goal_achievements: GoalAchievements

	# c/i
	start_inventory_from_pool: StartInventoryPool

dh_option_groups = [
	OptionGroup("Checks", [
		AchievementChecks,
		FusionChecks
	]),
	OptionGroup("Items", [
		PowerUpItems,
		PowerUpStartingLevels,
		CosmeticItems
	]),
	OptionGroup("Goal", [
		GoalStandardDistance,
		GoalClassicDistance,
		GoalBaelessDistance,
		GoalGachaDistance,
		GoalTheCellTime,
		GoalSpaceDistance,
		GoalSpaceScore,
		GoalAchievements
	])
]
