# BookAtrium

BookAtrium is a Windows ebook library, reader, organiser, and management application designed to make managing a personal ebook collection straightforward and enjoyable.

This repository is the official public home for BookAtrium downloads, documentation, support, feature requests, software issue reporting, and community discussions.

> The BookAtrium core application source code is private and is not stored in this public repository.

## Download BookAtrium

The latest stable version of BookAtrium will be available from the official GitHub Releases page.

[Download the latest BookAtrium release](../../releases/latest)

Only download BookAtrium from this repository or another location explicitly identified as an official BookAtrium download source.

## Project Status

BookAtrium is currently under active development and has not yet had its first public application release.

Public installers, release notes, and update information are published through this repository when application releases are created.

## Supported Platform

BookAtrium is currently being developed for:

- Windows 10
- Windows 11
- 64-bit systems

Support for additional platforms may be considered in the future.

## Documentation

BookAtrium documentation will include:

- Installation and update instructions
- Getting started guidance
- Library management
- Importing and organising ebooks
- Managing authors, series, tags, covers, and metadata
- Using the ebook reader
- Using the PDF reader
- Highlights, notes, and bookmarks
- Search and virtual libraries
- Backup and restore
- Web and OPDS access
- Plugin development
- Troubleshooting

The full user manual will be published here when the first public release is available.

### Plugin development (API 2.0)

Third-party plugins reference **only** the public NuGet package:

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.0.0" />
```

New third-party plugin packages use the `.bookplugin` extension.

- Source and package metadata: [`BookAtrium.PluginContracts`](BookAtrium.PluginContracts/)
- Guides: [`docs/plugins/sdk-2`](docs/plugins/sdk-2/)
- Reusable CI: [`.github/workflows/plugin-build.yml`](.github/workflows/plugin-build.yml)
- Official plugin catalogue: [`plugins/official/`](plugins/official/) and [`registries/official-plugins.json`](registries/official-plugins.json)
- Community catalogue (third-party metadata only): [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins)

The application core remains private. Third-party plugin projects should reference only `BookAtrium.PluginContracts`. Official BookAtrium plugins are developed privately and published here as catalogue metadata and release references.

## Support

Choose the support option that best matches your request:

- [Report a software bug](../../issues/new?template=01-bug-report.yml)
- [Request a feature](../../issues/new?template=02-feature-request.yml)
- [Report an installation or update problem](../../issues/new?template=03-installation-update.yml)
- [Report a documentation problem](../../issues/new?template=04-documentation.yml)
- [Apply to assist with BookAtrium development](../../issues/new?template=05-developer-interest.yml)
- [Ask a general question](../../discussions)

Before creating a new issue, please search the existing issues to see whether the problem or request has already been reported.

## Reporting a Problem

When reporting a problem, please include:

- Your BookAtrium version
- Your Windows version
- The affected area of the application
- A clear description of what happened
- The steps needed to reproduce the problem
- The result you expected
- Any relevant error message
- Screenshots where useful

Do not include passwords, licence keys, private ebooks, personal documents, database backups, customer information, or other confidential material.

## Feature Requests

Feature requests are welcome.

A useful feature request explains:

- What problem the feature would solve
- How the feature would improve BookAtrium
- An example of how it would be used
- Any alternatives or workarounds currently being used

Early ideas and general feedback can also be discussed in [GitHub Discussions](../../discussions).

## Official BookAtrium Plugins

BookAtrium publishes first-party plugins developed and maintained by the
BookAtrium project.

Official plugins are distributed through the official BookAtrium plugin
catalogue and are independently versioned. A plugin is updated only when its
installed package changes.

Official plugins are listed under:

- `plugins/official/`
- the BookAtrium official-plugin registry ([`registries/official-plugins.json`](registries/official-plugins.json))
- version-specific GitHub Releases

An official BookAtrium plugin that integrates with a third-party website or
service is not necessarily affiliated with or endorsed by that third party.
Refer to each plugin’s documentation for applicable notices.

### Current official plugins

| Plugin | Version | Category |
|--------|---------|----------|
| [Amazon US Kindle Store](plugins/official/stores/amazon-us-kindle-store/) | 1.0.4 | Store |

Amazon US Kindle Store is an official first-party BookAtrium plugin for searching Amazon US Kindle listings. It is not affiliated with, sponsored by, approved by, or endorsed by Amazon. Amazon and Kindle are trademarks of Amazon.com, Inc. or its affiliates.

## Third-Party Plugins

This section applies only to independently published third-party (community) plugins. It does not describe BookAtrium’s official first-party plugins.

BookAtrium supports independently developed third-party plugins through Plugin API 2.0.

Plugin developers will be free to decide whether their plugins are distributed without charge or sold commercially.

Third-party plugins will be distributed directly by their respective developers. BookAtrium does not operate an official third-party plugin marketplace or directory and does not host, sell, approve, certify, endorse, or provide support for independently developed plugins unless expressly stated otherwise.

Third-party catalogue metadata (not packages) may be listed separately in [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins). Official BookAtrium plugins are not submitted to that community catalogue.

Third-party plugin developers are responsible for:

- Hosting and distributing their plugins
- Pricing and payment arrangements
- Licensing and activation
- Refunds and customer service
- Documentation and technical support
- Updates and BookAtrium compatibility
- Security and privacy
- Compliance with applicable laws
- Compliance with third-party licences
- Any data collected or processed by their plugins

Users should install plugins only from developers and sources they trust.

Third-party plugins are subject to the plugin developer's own terms, licence, privacy policy, and support arrangements.

Third-party plugins use the community metadata registry at
[BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins).

Current valid community state may be an empty approved catalogue. At this time,
no community plugin has yet been publicly approved.

Normal plugin development does not require access to BookAtrium's private core
source code.

## Core Application Development

The BookAtrium core application is privately developed and proprietary.

Developers interested in helping with core application development may apply to become approved volunteer contributors.

Core development access is granted selectively and is not automatic. Applications may be assessed according to:

- Relevant technical experience
- Quality of previous work
- Areas in which assistance is needed
- Availability and reliability
- Security considerations
- Third-party component licensing requirements
- Willingness to follow the project's development standards

Approved contributors may be required to:

- Sign a non-disclosure agreement
- Sign an intellectual-property and contribution agreement
- Accept repository access and security requirements
- Comply with applicable third-party software licences
- Protect private source code and project information
- Work through assigned branches and reviewed pull requests
- Return or delete confidential material when access ends

Core development contributions are currently voluntary and unpaid.

Participation does not create an employment, contractor, partnership, agency, or payment relationship.

Approved contributors may receive public recognition for accepted work, including acknowledgement as an assistant developer or project contributor. The form and duration of recognition remain at the discretion of the project owner.

BookAtrium is currently intended to be available without charge. However, BookAtrium may introduce optional premium functionality, paid services, commercial editions, or another commercial model in the future.

Contributors must accept that approved contributions may continue to be used, modified, distributed, licensed, or commercialised by the BookAtrium project in accordance with the signed contribution agreement.

[Apply to assist with BookAtrium development](../../issues/new?template=05-developer-interest.yml)

## Releases and Updates

BookAtrium application releases are published through GitHub Releases.

Each release may include:

- The signed BookAtrium installer
- Release notes
- A list of changes and fixes
- The user manual
- File checksums where applicable

BookAtrium may use release information published in this repository to check for application updates.

> **Release-tag note:** This repository will eventually host both application releases and official plugin releases. Repository-wide “latest release” links become ambiguous once plugin tags are published here. Prefer version-specific application tags (for example `bookatrium-v1.0.0`) and version-specific plugin tags (for example `plugin-store-amazon-us-kindle-v1.0.4`). Plugin download metadata must never use `/releases/latest/download/...`. The download link above still points at `/releases/latest` only while no public application release exists yet; it should be replaced with a version-specific application URL when the first application release is published.

## Security

Do not report security vulnerabilities through public GitHub Issues or Discussions.

Please read the [Security Policy](SECURITY.md) for the current security-reporting process.

## Privacy

Public issues and discussions can be viewed by anyone.

Never submit:

- Passwords
- Licence keys
- Personal identification information
- Private ebook files
- Customer or business information
- Database backups
- Private source code
- Confidential project material
- Unedited diagnostic files containing sensitive data
- Signing certificates or API credentials

Always review screenshots, logs, and diagnostic information before uploading them publicly.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting issues, participating in Discussions, developing plugins, or applying to help with core development.

## Licence

The BookAtrium core application is private and proprietary.

Plugin contracts, samples, and documentation may use separate licences where indicated.

The absence of an open-source licence for the core application does not grant permission to copy, modify, redistribute, or commercially use the private BookAtrium source code.

## Software Licence

BookAtrium is proprietary software distributed under the
[BookAtrium End-User Licence Agreement](EULA.md).

The Software may currently be used without charge, but it is not open-source software.

## BookAtrium Community

- [Issues](../../issues)
- [Discussions](../../discussions)
- [Releases](../../releases)
- [Contribution Guidelines](CONTRIBUTING.md)
- [Security Policy](SECURITY.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)
