from typing import Optional, List, Any

from .item import *
from .location import *
from .regions import DHRegion
from .objects import DHObject
from .abilities import DHAbility
from .recipes import DHRecipe
from .powerups import DHPowerUp

GAME = "Delivering Hope"

dh_items = [
	# Game modes
	*(
		DHItem(DHItemType.GAME_MODE, region)
		for region in DHRegion
		if region.value > 0
	),

	# Objects
	*(
		DHItem(DHItemType.OBJECT, object_)
		for object_ in DHObject
	),

	# Abilities
	*(
		DHItem(DHItemType.ABILITY, ability)
		for ability in DHAbility
	),

	# Recipes
	*(
		DHItem(DHItemType.RECIPE, recipe)
		for recipe in DHRecipe
	),

	# Power Ups
	*(
		DHItem(DHItemType.POWERUP, powerup)
		for powerup in DHPowerUp
	),

	# Space Upgrades
	DHItem(DHItemType.SPACE, (0, "super divorce papers")),
	DHItem(DHItemType.SPACE, (1, "super last hope")),
	DHItem(DHItemType.SPACE, (2, "double strawberry")),
	DHItem(DHItemType.SPACE, (3, "Myth Reunion")),
	DHItem(DHItemType.SPACE, (4, "Eternal Promise")),

	# Progressive Items
	DHItem(DHItemType.PROGRESSIVE, (0, "Horizontal Power"), 4),
	DHItem(DHItemType.PROGRESSIVE, (1, "Vertical Power"), 4),
	DHItem(DHItemType.PROGRESSIVE, (2, "Extra Charge Slot"), 1),
	DHItem(DHItemType.PROGRESSIVE, (4, "Calli Yeet"), 4),
	DHItem(DHItemType.PROGRESSIVE, (5, "Meatball Resist"), 4),
	DHItem(DHItemType.PROGRESSIVE, (6, "Bounce Power"), 4),
	DHItem(DHItemType.PROGRESSIVE, (7, "Double Soda"), 4),
	DHItem(DHItemType.PROGRESSIVE, (11, "Rocket Power"), 4),
	DHItem(DHItemType.PROGRESSIVE, (12, "Extend Flight"), 4),
	DHItem(DHItemType.PROGRESSIVE, (14, "Kronii Power"), 4),
	DHItem(DHItemType.PROGRESSIVE, (15, "Mumei Countdown"), 2),
	DHItem(DHItemType.PROGRESSIVE, (19, "Kaela Bonk"), 4),
	DHItem(DHItemType.PROGRESSIVE, (20, "Phoenix Fire"), 4),
	DHItem(DHItemType.PROGRESSIVE, (21, "Lucky Dice"), 2),
	DHItem(DHItemType.PROGRESSIVE, (45, "Shark Tail"), 3),
	DHItem(DHItemType.PROGRESSIVE, (46, "Ina's Portal"), 3),
	DHItem(DHItemType.PROGRESSIVE, (47, "Fauna Slap"), 3),
	DHItem(DHItemType.PROGRESSIVE, (48, "Tofu Resist"), 3),
	DHItem(DHItemType.PROGRESSIVE, (49, "Triple Soda"), 3),
	DHItem(DHItemType.PROGRESSIVE, (51, "Kobo Bonk"), 3),

	DHItem(DHItemType.PROGRESSIVE, (100, "progressive strawberry"), 2),
	DHItem(DHItemType.PROGRESSIVE, (101, "progressive hot sauce"), 2),
	DHItem(DHItemType.PROGRESSIVE, (102, "progressive last hope"), 2),
	DHItem(DHItemType.PROGRESSIVE, (103, "progressive divorce papers"), 2),

	# Filler
	DHItem(DHItemType.FILLER, (6, "Hope Soda")),
	*(
		DHItem(DHItemType.FILLER, (5 + recipe.value, recipe.name()))
		for recipe in DHRecipe
	),
	DHItem(DHItemType.FILLER, (100, "50 Hope Stones")),
	DHItem(DHItemType.FILLER, (101, "75 Hope Stones")),
	#DHItem(DHItemType.FILLER, (1, "free space flight")),

	# Cosmetics
	DHItem(DHItemType.COSMETIC_BODY, (11, "ancient book")),
	DHItem(DHItemType.COSMETIC_BODY, (15, "dakimakura")),
	DHItem(DHItemType.COSMETIC_BODY, (2, "cat tail")),
	DHItem(DHItemType.COSMETIC_BODY, (14, "dango")),
	DHItem(DHItemType.COSMETIC_BODY, (7, "golden apple")),
	DHItem(DHItemType.COSMETIC_BODY, (5, "key necklace")),
	DHItem(DHItemType.COSMETIC_BODY, (8, "lamp")),
	DHItem(DHItemType.COSMETIC_BODY, (1, "microphone")),
	DHItem(DHItemType.COSMETIC_BODY, (13, "pocket watch")),
	DHItem(DHItemType.COSMETIC_BODY, (6, "big ribbon")),
	DHItem(DHItemType.COSMETIC_BODY, (12, "Death-sensei")),
	DHItem(DHItemType.COSMETIC_BODY, (9, "shark tail")),
	DHItem(DHItemType.COSMETIC_BODY, (3, "\"Hope Soda\" cosmetic")),
	DHItem(DHItemType.COSMETIC_BODY, (10, "warrior shield")),
	DHItem(DHItemType.COSMETIC_BODY, (4, "Yatagarasu")),
	DHItem(DHItemType.COSMETIC_HEAD, (10, "time hairpin")),
	DHItem(DHItemType.COSMETIC_HEAD, (15, "bloom and gloom")),
	DHItem(DHItemType.COSMETIC_HEAD, (2, "cat ears")),
	DHItem(DHItemType.COSMETIC_HEAD, (3, "cutie hairpin")),
	DHItem(DHItemType.COSMETIC_HEAD, (9, "shark hairpins")),
	DHItem(DHItemType.COSMETIC_HEAD, (13, "priestess tiara")),
	DHItem(DHItemType.COSMETIC_HEAD, (7, "kirin branches")),
	DHItem(DHItemType.COSMETIC_HEAD, (6, "Kronii hairstyle")),
	DHItem(DHItemType.COSMETIC_HEAD, (4, "limiter")),
	DHItem(DHItemType.COSMETIC_HEAD, (5, "head feathers")),
	DHItem(DHItemType.COSMETIC_HEAD, (8, "rat ears")),
	DHItem(DHItemType.COSMETIC_HEAD, (1, "sunglasses")),
	DHItem(DHItemType.COSMETIC_HEAD, (11, "tenchou hat")),
	DHItem(DHItemType.COSMETIC_HEAD, (12, "tiara")),
	DHItem(DHItemType.COSMETIC_HEAD, (14, "tofu")),
	DHItem(DHItemType.COSMETIC_TRAIL, (5, "banana trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (11, "berries trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (3, "bubble trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (15, "cheese trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (10, "clock trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (8, "code 116 trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (2, "crystal trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (7, "iris flower trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (13, "KFP trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (14, "magnifier trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (4, "music trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (9, "planet trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (12, "skull trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (1, "star trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (6, "tofu trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (17, "pizza trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (18, "flowers trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (16, "Tako trail")),
	DHItem(DHItemType.COSMETIC_BODY, (16, "archiver pen")),
	DHItem(DHItemType.COSMETIC_BODY, (19, "blue claws")),
	DHItem(DHItemType.COSMETIC_BODY, (18, "jewel")),
	DHItem(DHItemType.COSMETIC_BODY, (20, "pink claws")),
	DHItem(DHItemType.COSMETIC_BODY, (17, "tuning fork")),
	DHItem(DHItemType.COSMETIC_HEAD, (16, "archiver hairpin")),
	DHItem(DHItemType.COSMETIC_HEAD, (19, "blue headband")),
	DHItem(DHItemType.COSMETIC_HEAD, (17, "demon horn")),
	DHItem(DHItemType.COSMETIC_HEAD, (20, "pink headband")),
	DHItem(DHItemType.COSMETIC_HEAD, (18, "crystal crown")),
	DHItem(DHItemType.COSMETIC_TRAIL, (22, "\"Ruffian A\" trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (23, "\"Ruffian B\" trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (19, "Yorick trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (20, "Jailbird trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (21, "moai trail")),
	DHItem(DHItemType.COSMETIC_BODY, (24, "artist pen")),
	DHItem(DHItemType.COSMETIC_BODY, (22, "automaton gear")),
	DHItem(DHItemType.COSMETIC_BODY, (23, "Gremlin tail")),
	DHItem(DHItemType.COSMETIC_BODY, (21, "Thorn")),
	DHItem(DHItemType.COSMETIC_HEAD, (21, "Liz hairpin")),
	DHItem(DHItemType.COSMETIC_HEAD, (22, "automaton key")),
	DHItem(DHItemType.COSMETIC_HEAD, (23, "giant ahoge")),
	DHItem(DHItemType.COSMETIC_HEAD, (24, "goggles")),
	DHItem(DHItemType.COSMETIC_TRAIL, (26, "fan trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (24, "flame trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (25, "Otomo trail")),
	DHItem(DHItemType.COSMETIC_TRAIL, (27, "paint trail")),
]

dh_locations = [
	*(
		DHLocation(DHLocationType.ACHIEVEMENT, i, name, default, requirements)
		for i, (name, default, *requirements) in enumerate(
			(
				("Hope has Ascended", True, (DHObject.MUMEI, DHObject.KRONII)),
				("See The World", True),
				("Hope is in the Air", True),
				("Free Yeets", True, DHObject.MORI),
				("Bounce and Fly", True, DHObject.BANANA),
				("Gliding Hope", True),
				("High Hope", True, (DHObject.MUMEI, DHObject.KRONII)),
				("Long Range Hope", True),
				("Meatball-chan", True),
				("Sweet Nectar", True),
				("Third Time's the Charm", True),
				("Unlimited Yeet Works", True, DHObject.MORI),
				("Can't get over Bae", False),
				("Jail Time", True, DHObject.MONOPOLY),
				("Taxes", True, DHObject.MONOPOLY),
				("Superchat", True, DHObject.MONOPOLY),
				("Hope Express", True, DHObject.MONOPOLY),
				("Super Glider", True, DHObject.GLIDER),
				("Rocket Power", True, DHObject.JETPACK),
				("Soda Hunter", True, DHAbility.DOWNBOOST),
				("Gravity", True),
				("Till the End of Me", True, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII)),
				("Caesura of Despair", True, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII), DHAbility.DOWNBOOST),
				("Berserker", True, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII), DHAbility.DOWNBOOST),
				("HERE COMES HOPE", True, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII)),
				("Flower of hope", True, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII)),
				("One Step at a Time", True, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII)),
				("Antigravity", True, DHRegion.BAELESS, (DHObject.MUMEI, DHObject.KRONII)),
				("Unstoppable", True, DHRegion.BAELESS),
				("guh", True, DHRegion.BAELESS, DHObject.MORI),
				("Hoot Hoot", True, DHRegion.BAELESS, DHObject.MUMEI),
				("GWAK", True, DHRegion.BAELESS, DHObject.KRONII),
				("Fever Night", True, DHRegion.BAELESS, DHObject.KIARA),
				("A whole new HOPE", True, DHAbility.FUSION, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII)),
				("Zooming HOPE", True, DHObject.JETPACK),
				("Hot Wings", True, DHObject.KIARA),
				("Holy Shitto", True, DHRegion.BAELESS, DHObject.MORI),
				("Civilization", True, DHRegion.BAELESS, DHObject.MUMEI),
				("GWAK!?", True, DHRegion.BAELESS, DHObject.KRONII),
				("SPARKS", True, DHRegion.BAELESS, DHObject.KIARA),
				("Tokyo Tower", True, (DHObject.MUMEI, DHObject.KRONII)),
				("MORE POWEER!", True, DHAbility.FUSION, DHAbility.BEACH, DHObject.MORI, DHObject.MUMEI, DHObject.KRONII, DHObject.INA, DHObject.GURA, DHObject.FAUNA),
				("Fuu-sion!", True, DHAbility.FUSION, DHObject.MORI, DHObject.MUMEI, DHObject.KRONII),
				(None, None),
				(None, None),
				("Endurance", False),
				("you, sleb", False),
				("Fun game", False),
				("Well equipped", True),
				("Potionmaker", True, (*DHRecipe,)),
				("Ancient Knowledge", False, *DHRecipe),
				("Frequent Buyer", True, DHAbility.SHOP),
				("Deep Pockets", False, DHAbility.SHOP),
				("Apprentice Detective", False, DHAbility.FUSION, *DHObject),
				("Great Detective", False, DHAbility.FUSION, *DHObject, *DHPowerUp, DHAbility.BEACH, DHAbility.DOWNBOOST),
				("Training", True),
				("Collector", False, DHObject.MORI, DHObject.MONOPOLY, DHObject.MUMEI, DHObject.KRONII, DHRegion.BAELESS, DHRegion.GACHA, DHAbility.DOWNBOOST),
				("Professional", False, DHObject.MORI, DHObject.MONOPOLY, DHObject.MUMEI, DHObject.KRONII, DHRegion.BAELESS, DHRegion.GACHA, DHAbility.DOWNBOOST),
				("Daredevil", True, DHObject.MORI),
				("Boost and Bonk", True, DHAbility.DOWNBOOST),
				("Supersonic", True, DHObject.JETPACK),
				("Hypersonic", True, DHObject.JETPACK),
				("I need it", True),
				("I don't need it", True),
				("So close yet so far", True),
				("Extra Flex", True, DHPowerUp.EXTRA_CHARGE, DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII)),
				("Extreme Fusion", True, DHAbility.FUSION, *DHObject, DHAbility.BEACH, DHAbility.DOWNBOOST),
				("Luck and Skill", True, DHRegion.BAELESS, DHObject.JETPACK, DHObject.MORI),
				("Oh hi", True, DHRegion.BAELESS, DHAbility.DOWNBOOST, DHObject.MUMEI),
				("Feeling Lucky", True, DHRegion.GACHA),
				("Incredible Luck!", True, DHRegion.GACHA),
				("Sixth sense", True, DHRegion.GACHA),
				("Perfect Yeet!", False),
				("Beyond the Clouds", True, (DHObject.MUMEI, DHObject.KRONII)),
				("DizzyRyS", True, DHObject.GURA, DHAbility.BEACH),
				("Inaception", True, DHObject.INA, DHAbility.BEACH),
				("Choo Choo!", True, DHRegion.BAELESS, DHObject.MONOPOLY),
				("No Escape", True, DHObject.MONOPOLY),
				("One button only", True),
				("The Cell", True, DHRegion.BAELESS, DHObject.MORI),
				("Space", True, DHRegion.THE_CELL, (DHObject.NERISSA, DHObject.SHIORI), (DHObject.NERISSA, DHObject.FUWAMOCO), (DHObject.FUWAMOCO, DHObject.SHIORI)),
			),
			start = 1
		)
		if name
	),
	*(
		DHLocation(DHLocationType.SHOP, i + 6, f"Shop item {i + 1}", True, [])
		for i in range(7)
	),
	*(
		DHLocation(DHLocationType.SPACE, i, f"Space unlock {i + 1}", True, [DHRegion.SPACE])
		for i in range(5)
	),
	*(
		DHLocation(DHLocationType.FUSION, i, name, True, [DHObject.MORI, (DHObject.MUMEI, DHObject.KRONII), part1, part2, DHAbility.BEACH if beach else None])
		for i, name, part1, part2, beach in (
			(22, "Takamori", DHObject.MORI, DHObject.KIARA, False),
			(23, "Kronmei", DHObject.KRONII, DHObject.MUMEI, False),
			(24, "The Plague", DHObject.MORI, DHObject.MUMEI, False),
			(25, "Holobirds", DHObject.KIARA, DHObject.MUMEI, False),
			(26, "Time's Up", DHObject.MORI, DHObject.KRONII, False),
			(27, "Endless Cycle", DHObject.KIARA, DHObject.KRONII, False),
			(28, "Diagonal Boost", DHPowerUp.SODA_BOOST, None, False),
			(29, "Large Soda", DHPowerUp.SODA_QUANTITY, DHPowerUp.EXTRA_CHARGE, False),
			(30, "Soda to go", DHObject.JETPACK, None, False),
			(31, "Soda Snipe", DHPowerUp.SODA_QUANTITY, DHAbility.DOWNBOOST, False),
			(32, "Yummy Food", DHObject.BANANA, DHPowerUp.RESIST_SLOW, False),
			(33, "Banana Soda", DHObject.BANANA, DHPowerUp.SODA_QUANTITY, False),
			(34, "Rocket Train", DHObject.MONOPOLY, DHObject.JETPACK, False),
			(35, "Double Roll", DHObject.MONOPOLY, DHPowerUp.EXTRA_CHARGE, False),
			(36, "Surprise Meatball", DHObject.MUMEI, DHPowerUp.RESIST_SLOW, False),
			(37, "Hot Sauce to go", (DHObject.JETPACK, DHObject.GLIDER), DHPowerUp.RESIST_SLOW, False),
			(38, "Extra fuel", DHObject.MUMEI, DHObject.JETPACK, False),
			(39, "Banana to go", DHObject.BANANA, (DHObject.JETPACK, DHObject.GLIDER), False),
			(40, "Elamei", DHObject.MUMEI, DHAbility.DOWNBOOST, False),
			(41, "GSH", DHObject.MONOPOLY, DHAbility.DOWNBOOST, False),
			(42, "Phoenix Slam", DHObject.KIARA, DHAbility.DOWNBOOST, False),
			(43, "Elamori", DHObject.MORI, DHAbility.DOWNBOOST, False),
			(44, "Timesmith", DHObject.KRONII, DHAbility.DOWNBOOST, False),
			(53, "Banana Soda Plus", DHPowerUp.SODA_QUANTITY, DHObject.BANANA, True),
			(54, "Soda Snipe Plus", DHPowerUp.SODA_QUANTITY, DHAbility.DOWNBOOST, True),
			(55, "Yummy Food Plus", DHObject.BANANA, DHPowerUp.TOFU_RESIST, True),
			(56, "Surprise Tofu", DHObject.GURA, DHPowerUp.TOFU_RESIST, True),
			(57, "Inasame", DHObject.GURA, DHObject.INA, True),
			(58, "Sametori", DHObject.GURA, DHObject.KIARA, True),
			(59, "Samekirin", DHObject.GURA, DHObject.FAUNA, True),
			(60, "Takotori", DHObject.INA, DHObject.KIARA, True),
			(61, "Takokirin", DHObject.INA, DHObject.FAUNA, True),
			(62, "Fire Slap", DHObject.KIARA, DHObject.FAUNA, True),
			(63, "Violet", DHAbility.DOWNBOOST, DHObject.INA, True),
			(64, "Shaman Whisper", DHAbility.DOWNBOOST, DHObject.FAUNA, True),
			(65, "Wet Shark", DHAbility.DOWNBOOST, DHObject.GURA, True),
			(66, "Mommy Kiwawa", DHAbility.DOWNBOOST, DHObject.KIARA, True)
		)
	)
]

dh_item_name_to_object = {
	item.name: item
	for item in dh_items
}

dh_item_name_to_id = {
	item.name: item.id
	for item in dh_items
}

dh_location_name_to_id = {
	location.name: location.id
	for location in dh_locations
}
