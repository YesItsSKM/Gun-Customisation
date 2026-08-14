# Legacy Asset Provenance Register

This register is a release-control aid, not legal advice. “Likely match” is not enough for distribution: retain the original download receipt/archive and exact license text for every included third-party asset.

| Local content | Evidence found | Status / release action |
|---|---|---|
| `Guns/m4-modular` | Local names and modular parts closely match [Free - M4 Modular Kit Gun by Karnaval](https://sketchfab.com/3d-models/free-m4-modular-kit-gun-1665416c071747bb9c20aa652849b579), listed as CC Attribution. | Likely match only. Verify the downloaded archive/hash, record author/link/license, and add attribution before any redistribution. |
| `Guns/overkill-handgun` | Local name/textures closely match [Overkill Handgun by Three_dots](https://sketchfab.com/3d-models/overkill-handgun-f5d0e86fd5de40fb9302859ffe5ea5b6), listed as CC Attribution. | Likely match only. Verify exact archive and creator attribution; otherwise exclude. |
| `Guns/mp7-smg` | Source filename contains `MP7_for_Sketchfab`, but no exact listing/license was reliably resolved. | Exclude until source and redistribution terms are documented. |
| `Guns/pistol`, `Guns/sniper` | No bundled provenance or exact source resolved. | Exclude until documented. |
| Workshop props and textures | No license/readme/receipt files are bundled. | Exclude from commercial package. Research each source only if retaining the legacy room is worth the cost. |
| `studio_small_09_4k.hdr` | Filename matches [Studio Small 09 by Sergej Majboroda on Poly Haven](https://polyhaven.com/a/studio_small_09), published under CC0. | Strong match. Preserve the source URL and author in final notices; confirm the local file came from that download. |
| `Fonts/Afacad_Flux` | Filenames match [Google Fonts’ Afacad Flux family](https://github.com/google/fonts/tree/main/ofl/afacadflux), licensed under SIL OFL 1.1. | OFL text has now been restored beside the font files. Keep that license with any redistributed copy. |
| `Fonts/Consolas` | Filenames are Microsoft Consolas; [Microsoft identifies the family as Microsoft copyrighted and directs redistribution to separate licensing](https://learn.microsoft.com/en-us/typography/font-list/consolas). | All scene/UI references were replaced with Afacad Flux. The unreferenced files remain in the recovery repo but must not be exported; delete them from any future redistributable demo. |
| TextMesh Pro bundled content | Liberation Sans OFL and EmojiOne attribution files are present in their original folders. | Re-check what Unity permits/needs in an exported Asset Store package; avoid exporting unnecessary TMP resources. |

## Commercial default

The scoped exporter includes only `Assets/SKM/ModularWeaponCustomisation`, which currently embeds no third-party art, fonts, audio, or source. Treat every legacy item outside that root as excluded unless this register is upgraded to an exact, documented approval.
