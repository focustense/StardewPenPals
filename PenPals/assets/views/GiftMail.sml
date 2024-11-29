<lane layout="85%[1200..] 80%[800..]"
      orientation="vertical"
      horizontal-content-alignment="middle">
    <banner background={@Mods/StardewUI/Sprites/BannerBackground}
            background-border-thickness="48,0"
            padding="12"
            text={#GiftMailMenu.Title} />
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
            <scrollable layout="stretch">
                <grid layout="stretch content"
                      item-layout="length: 160"
                      item-spacing="16, 16"
                      padding="16, 16, 16, 0"
                      horizontal-item-alignment="middle">
                    <recipient *repeat={:Recipients} />
                </grid>
            </scrollable>
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
               click=|^SelectRecipient(this)|>
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
            <frame *if={:HasPendingGift}
                   *context={:PendingGift}
                   layout="32px"
                   margin="2, 0, 0, 2">
                <item star-layout="12px" />
            </frame>
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