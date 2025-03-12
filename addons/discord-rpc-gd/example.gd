extends Node
func _ready():
	DiscordRPC.app_id = 1335860356379312178 # Application ID
	DiscordRPC.details = "Writhing in wait"
	DiscordRPC.large_image = "placeholder" # Image key from "Art Assets"
	DiscordRPC.large_image_text = "Writhing in wait"
	# DiscordRPC.small_image = "boss" # Image key from "Art Assets"
	# DiscordRPC.small_image_text = "Fighting the end boss! D:"

	DiscordRPC.start_timestamp = int(Time.get_unix_time_from_system()) # "02:46 elapsed"
	# DiscordRPC.end_timestamp = int(Time.get_unix_time_from_system()) + 3600 # +1 hour in unix time / "01:00:00 remaining"

	DiscordRPC.refresh() # Always refresh after changing the values!
	
	print(DiscordRPC.get_is_discord_working())
