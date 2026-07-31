# Official plugin publication report

Generated (UTC): 2026-07-31T16:59:12.7846081+00:00
Plugin count: 67
Catalog: `official-plugins.json`
Packages directory: `registries/packages/`
Baseline release tag: `official-plugins-baseline-v1.0.0`

| Category | Count |
| --- | ---: |
| AuthorMetadataSource | 1 |
| ConversionInput | 8 |
| ConversionOutput | 10 |
| FileType | 1 |
| InputProfile | 1 |
| MetadataReader | 18 |
| MetadataSource | 18 |
| MetadataWriter | 7 |
| OutputProfile | 1 |
| Store | 2 |

## Largest packages

| Plugin ID | Size (bytes) |
| --- | ---: |
| `com.practicore.bookatrium.conversionoutput.pdf` | 3,947,096 |
| `com.practicore.bookatrium.conversioninput.pdf` | 3,939,071 |
| `com.practicore.bookatrium.conversioninput.kindle` | 385,511 |
| `com.practicore.bookatrium.conversioninput.kepub` | 374,712 |
| `com.practicore.bookatrium.conversioninput.epub` | 373,871 |
| `com.practicore.bookatrium.conversioninput.html` | 373,653 |
| `com.practicore.bookatrium.conversioninput.docx` | 373,597 |
| `com.practicore.bookatrium.conversionoutput.azw3` | 373,565 |
| `com.practicore.bookatrium.conversionoutput.mobi` | 373,523 |
| `com.practicore.bookatrium.conversionoutput.kepub` | 373,040 |
| `com.practicore.bookatrium.conversioninput.txt` | 372,250 |
| `com.practicore.bookatrium.conversioninput.fb2` | 372,208 |
| `com.practicore.bookatrium.conversionoutput.docx` | 371,612 |
| `com.practicore.bookatrium.conversionoutput.epub` | 371,283 |
| `com.practicore.bookatrium.conversionoutput.lrf` | 369,869 |

## Hosting

1. Upload every `.bookplugin` under `registries/packages/` to a GitHub Release
   tagged `official-plugins-baseline-v1.0.0` on `lgdeysel1980/BookAtrium`
   (Amazon US Kindle Store keeps its existing per-plugin release tag).
2. Commit `registries/official-plugins.json` and `registries/official-plugin-inventory.json`
   to the public BookAtrium `main` branch so the production client URL resolves.
3. Verify `BOOKATRIUM_OFFICIAL_PLUGIN_REGISTRY_URL` (optional override) or the default
   raw GitHub URL loads the updated catalogue.
