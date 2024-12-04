<lane layout="85%[1200..] 80%[800..]"
      orientation="vertical"
      horizontal-content-alignment="middle">
    <panel horizontal-content-alignment="end"
           vertical-content-alignment="middle">
        <banner background={@Mods/StardewUI/Sprites/BannerBackground}
                background-border-thickness="48,0"
                padding="12, 12, 80, 12"
                text={#GiftMailMenu.Title} />
        <frame margin="60, 0">
            <image layout="48px"
                   sprite={@Mods/focustense.PenPals/Sprites/Cursors:MagnifyingGlass}
                   tint={FilterButtonTint}
                   tooltip={#GiftMailMenu.Filters.Toggle.Tooltip}
                   focusable="true"
                   left-click=|ToggleFilters()| />
        </frame>
    </panel>
    <lane layout="stretch" vertical-content-alignment="middle">
        <frame *context={:Gift}
               layout="84px"
               padding="20"
               horizontal-content-alignment="middle"
               vertical-content-alignment="middle"
               background={@Mods/StardewUI/Sprites/ControlBorder}>
            <item star-layout="24px" />
        </frame>
        <image margin="16, 0, 0, 0"
               sprite={@Mods/StardewUI/Sprites/CaretRight} />
        <frame layout="stretch"
               background={@Mods/StardewUI/Sprites/MenuBackground}
               border={@Mods/StardewUI/Sprites/MenuBorder}
               border-thickness="36, 36, 40, 36">
            <lane layout="stretch" orientation="vertical">
                <lane *if={FiltersVisible}
                      *context={:Filters}
                      layout="stretch content"
                      padding="16, 8"
                      vertical-content-alignment="middle"
                      z-index="1">
                    <label margin="0, 0, 8, 0" text={#GiftMailMenu.Filters.SearchText.Label} />
                    <panel layout="content stretch" vertical-content-alignment="middle">
                        <textinput layout="250px 60px"
                                   margin="0, 0, 8, 0"
                                   max-length="25"
                                   text={<>SearchText} />
                    </panel>
                    <spacer layout="stretch 0px" />
                    <button *switch={MinTaste}
                            margin="0, 0, 4, 0"
                            tooltip={#GiftMailMenu.Filters.Taste.Tooltip}
                            left-click=|ToggleTaste()|>
                        <image *case="Neutral"
                               layout="32px 36px"
                               margin="2"
                               vertical-alignment="middle"
                               sprite={@Mods/focustense.PenPals/Sprites/Emojis:Happy}
                               tint={TasteButtonTint} />
                        <image *case="Like"
                               layout="32px 36px"
                               margin="2"
                               vertical-alignment="middle"
                               sprite={@Mods/focustense.PenPals/Sprites/Emojis:Happy}
                               tint={TasteButtonTint} />
                        <image *case="Love"
                               layout="32px 36px"
                               margin="2"
                               vertical-alignment="middle"
                               sprite={@Mods/focustense.PenPals/Sprites/Emojis:Grin}
                               tint={TasteButtonTint} />
                    </button>
                    <button margin="0, 0, 4, 0"
                            tooltip={#GiftMailMenu.Filters.Quests.Tooltip}
                            left-click=|ToggleQuests()|>
                        <image layout="20px 36px"
                               margin="2"
                               horizontal-alignment="middle"
                               vertical-alignment="middle"
                               sprite={@Mods/focustense.PenPals/Sprites/Cursors:QuestIndicator}
                               tint={QuestButtonTint} />
                    </button>
                    <button margin="0, 0, 4, 0"
                            tooltip={#GiftMailMenu.Filters.Birthdays.Tooltip}
                            left-click=|ToggleBirthdays()|>
                        <image layout="20px 36px"
                               margin="2"
                               horizontal-alignment="middle"
                               vertical-alignment="middle"
                               sprite={@Mods/focustense.PenPals/Sprites/Cursors:Balloon}
                               tint={BirthdayButtonTint} />
                    </button>
                    <button margin="0, 0, 4, 0"
                            tooltip={#GiftMailMenu.Filters.Reset.Tooltip}
                            left-click=|Clear()|>
                        <image layout="32px"
                               margin="0, 4"
                               horizontal-alignment="middle"
                               vertical-alignment="middle"
                               sprite={@Mods/focustense.PenPals/Sprites/Cursors:BigRedX} />
                    </button>
                </lane>
                <image *if={FiltersVisible}
                       layout="stretch content"
                       fit="stretch"
                       margin="-36, -20, -40, -20"
                       sprite={@Mods/StardewUI/Sprites/MenuHorizontalDivider} />
                <scrollable layout="stretch">
                    <grid layout="stretch content"
                          item-layout="length: 160"
                          item-spacing="16, 16"
                          padding="16, 16, 16, 0"
                          horizontal-item-alignment="middle">
                        <recipient *repeat={Recipients} />
                    </grid>
                </scrollable>
            </lane>
        </frame>
        <spacer layout="120px" />
    </lane>
    <spacer layout="0px 64px" />
</lane>

<template name="recipient">
    <frame background={@Mods/focustense.PenPals/Sprites/Cursors:PortraitFrame}
           background-tint={:BackgroundTint}>
        <panel margin="16, 16, 16, 10"
               tooltip={:TooltipText}
               focusable="true"
               left-click=|^SelectRecipient(this)|>
            <image layout="128px"
                   vertical-alignment="end"
                   sprite={:Portrait}
                   tint={:PortraitTint} />
            <panel *switch={:Reaction}
                   layout="stretch"
                   margin="0, 0, 2, 2"
                   horizontal-content-alignment="end"
                   vertical-content-alignment="end">
                <reaction *case="Love" sprite={@Mods/focustense.PenPals/Sprites/Emojis:Grin} />
                <reaction *case="Like" sprite={@Mods/focustense.PenPals/Sprites/Emojis:Happy} />
                <reaction *case="Dislike" sprite={@Mods/focustense.PenPals/Sprites/Emojis:Unhappy} />
                <reaction *case="Hate" sprite={@Mods/focustense.PenPals/Sprites/Emojis:Angry} />
            </panel>
            <lane>
                <frame *if={:HasPendingGift}
                       *context={:PendingGift}
                       layout="32px"
                       margin="2, 0, 2, 2">
                    <item star-layout="12px" />
                </frame>
                <image *if={:HasPendingQuest}
                       layout="32px"
                       margin="2, 0, 2, 0"
                       sprite={@Mods/focustense.PenPals/Sprites/Cursors:QuestButton}
                       shadow-alpha="0.25"
                       shadow-offset="-2, 2"
                       tooltip={:PendingQuest} />
                <image *if={:HasBirthday}
                       layout="32px"
                       margin="2, 0, 2, 0"
                       sprite={@Mods/focustense.PenPals/Sprites/Cursors:Balloon}
                       shadow-alpha="0.25"
                       shadow-offset="-2, 2"
                       tooltip={:BirthdayTooltip} />
            </lane>
        </panel>
    </frame>
</template>

<template name="item">
    <panel *context={:Image}
           layout="stretch"
           vertical-content-alignment="end"
           tooltip={:Item}>
        <image layout="stretch"
               sprite={:Sprite}
               tint={:SpriteColor}
               shadow-alpha="0.25"
               shadow-offset="-2, 2" />
        <image *if={:HasTintSprite}
               sprite={:TintSprite}
               tint={:TintSpriteColor} />
        <frame *switch={:^Quality} layout={&star-layout}>
            <image *case="1" layout="stretch content" sprite={@Mods/focustense.PenPals/Sprites/Cursors:QualityStarSilver} />
            <image *case="2" layout="stretch content" sprite={@Mods/focustense.PenPals/Sprites/Cursors:QualityStarGold} />
            <image *case="4" layout="stretch content" sprite={@Mods/focustense.PenPals/Sprites/Cursors:QualityStarIridium} />
        </frame>
    </panel>
</template>

<template name="reaction">
    <image layout="27px" sprite={&sprite} tint={:ReactionTint} />
</template>