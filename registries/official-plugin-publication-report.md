# Official plugin publication report

Generated (UTC): 2026-08-01T12:24:24.4495735+00:00
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
| `com.practicore.bookatrium.conversionoutput.pdf` | 4,245,929 |
| `com.practicore.bookatrium.conversioninput.pdf` | 3,762,467 |
| `com.practicore.bookatrium.conversioninput.kindle` | 383,662 |
| `com.practicore.bookatrium.conversioninput.kepub` | 372,911 |
| `com.practicore.bookatrium.conversioninput.epub` | 372,079 |
| `com.practicore.bookatrium.conversioninput.html` | 371,842 |
| `com.practicore.bookatrium.conversioninput.docx` | 371,779 |
| `com.practicore.bookatrium.conversionoutput.azw3` | 371,764 |
| `com.practicore.bookatrium.conversionoutput.mobi` | 371,743 |
| `com.practicore.bookatrium.conversionoutput.kepub` | 371,228 |
| `com.practicore.bookatrium.conversioninput.txt` | 370,446 |
| `com.practicore.bookatrium.conversioninput.fb2` | 370,404 |
| `com.practicore.bookatrium.conversionoutput.docx` | 369,826 |
| `com.practicore.bookatrium.conversionoutput.epub` | 369,469 |
| `com.practicore.bookatrium.conversionoutput.lrf` | 368,075 |

## Hosting

1. Upload every `.bookplugin` under `registries/packages/` to a GitHub Release
   tagged `official-plugins-baseline-v1.0.0` on `lgdeysel1980/BookAtrium`
   (Amazon US Kindle Store keeps its existing per-plugin release tag).
2. Commit `registries/official-plugins.json` and `registries/official-plugin-inventory.json`
   to the public BookAtrium `main` branch so the production client URL resolves.
3. Verify `BOOKATRIUM_OFFICIAL_PLUGIN_REGISTRY_URL` (optional override) or the default
   raw GitHub URL loads the updated catalogue.
