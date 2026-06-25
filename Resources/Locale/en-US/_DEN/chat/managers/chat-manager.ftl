# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
chat-manager-entity-subtle-wrap-message = [italic]{ PROPER($entity) ->
    *[false] The {$entityName} {$message}[/italic]
    [true] {CAPITALIZE($entityName)} {$message}[/italic]
}

chat-manager-entity-subtle-ooc-wrap-message = [italic](OOC) {$entityName} {$message}[/italic]

# Chitinid Start
chat-speech-verb-name-chitinid = Chitinid
chat-speech-verb-chitinid-1 = clicks
chat-speech-verb-chitinid-2 = chitters
chat-speech-verb-chitinid-3 = hisses
chat-speech-verb-chitinid-4 = buzzes
# Chitinid End
