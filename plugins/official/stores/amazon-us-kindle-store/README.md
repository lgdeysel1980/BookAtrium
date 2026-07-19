# Amazon US Kindle Store

Official first-party BookAtrium plugin for searching Amazon US Kindle listings.

> This plugin is not affiliated with, sponsored by, approved by, or endorsed by Amazon. Amazon and Kindle are trademarks of Amazon.com, Inc. or its affiliates.

“Official” refers only to BookAtrium publisher status. It does not imply affiliation with Amazon.

## Purpose

Searches the normal Amazon.com US Kindle search page, returns store results BookAtrium can display, and opens selected product pages in the user's default browser.

Current published version: **1.0.4**

## Compatibility

| Requirement | Value |
|-------------|-------|
| Plugin API | 2.0 |
| Contracts package | `BookAtrium.PluginContracts` 2.0.0 |
| Minimum BookAtrium version | 1.0.0 |
| Platforms | Windows |

## Features

- Searches the normal Amazon US Kindle search page.
- Parses returned HTML into BookAtrium store results.
- Returns title, author, price, Kindle format, ASIN, product URL, and cover URL when Amazon provides them.
- Lets BookAtrium open the Amazon product page in the user's default browser when a result is activated.
- Multi-page result retrieval (up to the configured page and result limits).

The plugin does not use the Amazon Product Advertising API or any Amazon account integration.

## Limitations

Amazon changes its HTML frequently. The parser uses conservative selectors and skips malformed rows, but search results may temporarily be incomplete if Amazon changes markup or shows CAPTCHA/unusual pages.

The plugin does not guarantee exact edition matches, Kindle Unlimited eligibility, regional availability, current prices, or DRM status.

## Scope and safety

This plugin only searches and opens product pages. It does not:

- Request Amazon credentials.
- Access Amazon accounts or Kindle libraries.
- Add items to cart, initiate checkout, or purchase books.
- Download Kindle books.
- Handle or remove DRM.
- Automate browsers.
- Embed Amazon pages inside BookAtrium.
- Use affiliate tags.
- Send telemetry or analytics.

Search terms are sent directly to Amazon when the user searches.

## Network access

Declared hosts:

- `www.amazon.com`
- `amazon.com`
- `m.media-amazon.com`
- `images-na.ssl-images-amazon.com`
- `images-eu.ssl-images-amazon.com`

BookAtrium provides HTTP access and enforces the declared host allowlist.

## Privacy summary

The plugin does not collect personal information, telemetry, or Amazon credentials. Search terms go to Amazon.com under Amazon's policies. Product-page browsing happens in the user's default browser. See [PRIVACY.md](PRIVACY.md).

## Installation through BookAtrium

Install from BookAtrium's official plugin catalogue when that UI is available in your BookAtrium build.

You may also install a downloaded `.bookplugin` from BookAtrium Settings → Plugins.

## Manual download

The official package is hosted through the BookAtrium repository:

- Release: [plugin-store-amazon-us-kindle-v1.0.4](https://github.com/lgdeysel1980/BookAtrium/releases/tag/plugin-store-amazon-us-kindle-v1.0.4)
- Package: [`com.practicore.bookatrium.store.amazon-us-kindle-1.0.4.bookplugin`](https://github.com/lgdeysel1980/BookAtrium/releases/download/plugin-store-amazon-us-kindle-v1.0.4/com.practicore.bookatrium.store.amazon-us-kindle-1.0.4.bookplugin)

SHA-256:

`65040330d195f98597f8a0484c559020cd64b3669b75fc89befd09e6a9a65719`

Always use the version-specific BookAtrium release URL for new downloads. Do not use `/releases/latest/download/...`.

## Support

Plugin and BookAtrium support: [BookAtrium issues](https://github.com/lgdeysel1980/BookAtrium/issues)

Security: see [SECURITY.md](SECURITY.md) and the [BookAtrium security policy](https://github.com/lgdeysel1980/BookAtrium/blob/main/SECURITY.md).

## Release history

- Catalogue changelog: [CHANGELOG.md](CHANGELOG.md)
- Official BookAtrium release: [plugin-store-amazon-us-kindle-v1.0.4](https://github.com/lgdeysel1980/BookAtrium/releases/tag/plugin-store-amazon-us-kindle-v1.0.4)

## Licence

The already published v1.0.4 package and its included documentation are distributed under the [MIT License](LICENSE). That licence does not grant rights to Amazon trademarks, storefront design, product data, book metadata, or Amazon pages.

Official BookAtrium plugin implementation source is developed privately and is not published as open source. This catalogue page does not provide build-from-source instructions.
