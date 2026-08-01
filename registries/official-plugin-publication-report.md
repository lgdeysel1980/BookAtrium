# Official plugin publication report

Generated (UTC): 2026-08-01T13:46:04.7540106+00:00
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
| `com.practicore.bookatrium.conversionoutput.pdf` | 4,242,012 |
| `com.practicore.bookatrium.conversioninput.pdf` | 3,758,554 |
| `com.practicore.bookatrium.conversioninput.kindle` | 379,958 |
| `com.practicore.bookatrium.conversioninput.kepub` | 369,210 |
| `com.practicore.bookatrium.conversioninput.epub` | 368,377 |
| `com.practicore.bookatrium.conversioninput.html` | 368,139 |
| `com.practicore.bookatrium.conversioninput.docx` | 368,084 |
| `com.practicore.bookatrium.conversionoutput.azw3` | 368,069 |
| `com.practicore.bookatrium.conversionoutput.mobi` | 368,039 |
| `com.practicore.bookatrium.conversionoutput.kepub` | 367,525 |
| `com.practicore.bookatrium.conversioninput.txt` | 366,738 |
| `com.practicore.bookatrium.conversioninput.fb2` | 366,709 |
| `com.practicore.bookatrium.conversionoutput.docx` | 366,125 |
| `com.practicore.bookatrium.conversionoutput.epub` | 365,771 |
| `com.practicore.bookatrium.conversionoutput.lrf` | 364,376 |

## Hosting

1. Upload every `.bookplugin` under `registries/packages/` to a GitHub Release
   tagged `official-plugins-baseline-v1.0.0` on `lgdeysel1980/BookAtrium`
   (Amazon US Kindle Store keeps its existing per-plugin release tag).
2. Commit `registries/official-plugins.json` and `registries/official-plugin-inventory.json`
   to the public BookAtrium `main` branch so the production client URL resolves.
3. Verify `BOOKATRIUM_OFFICIAL_PLUGIN_REGISTRY_URL` (optional override) or the default
   raw GitHub URL loads the updated catalogue.
