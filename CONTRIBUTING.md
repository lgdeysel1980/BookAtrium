# Contributing to BookAtrium

Thank you for your interest in helping improve BookAtrium.

BookAtrium welcomes user feedback, issue reports, documentation corrections, feature suggestions, community participation, third-party plugin development, and applications from developers who may wish to assist with private core development.

The BookAtrium core application source code is private and proprietary. It is not hosted in this public repository.

## Ways to Contribute

You can contribute by:

- Reporting software problems
- Suggesting features and improvements
- Reporting installer or update problems
- Identifying documentation errors
- Participating in GitHub Discussions
- Helping other users
- Developing independent third-party plugins
- Applying to assist with private core application development

## Reporting Bugs

Use the structured Bug Report form:

[Report a bug](../../issues/new?template=01-bug-report.yml)

Before submitting:

- Search existing issues
- Confirm the problem still occurs in the latest available release
- Include the BookAtrium version
- Include your Windows version
- Include clear reproduction steps
- Explain the expected behaviour
- Include relevant error messages
- Remove all private and sensitive information

A bug report should normally describe one main problem.

## Requesting Features

Use the Feature Request form:

[Request a feature](../../issues/new?template=02-feature-request.yml)

A useful feature request explains:

- The problem or limitation
- The proposed improvement
- How the feature would be used
- Who would benefit
- Any alternatives or workarounds

Please submit separate requests for unrelated features.

Early ideas and general product feedback may be better suited to [GitHub Discussions](../../discussions).

## Installation and Update Problems

Use the Installation or Update Problem form:

[Report an installation or update problem](../../issues/new?template=03-installation-update.yml)

Please include:

- The currently installed version
- The version being installed
- Your Windows version
- The stage at which the process failed
- Any installer or updater error code
- Steps already attempted
- Relevant screenshots or logs

Only download BookAtrium from an official BookAtrium source.

## Documentation Corrections

Use the Documentation Problem form:

[Report a documentation problem](../../issues/new?template=04-documentation.yml)

Please identify:

- The manual, page, section, or application screen
- What is incorrect, missing, unclear, or outdated
- The correction you recommend, where possible

## General Questions

Use GitHub Discussions for:

- Questions about using BookAtrium
- General feedback
- Community assistance
- Early ideas
- Usage tips
- Conversations that are not specific development tickets

[Open BookAtrium Discussions](../../discussions)

## Third-Party Plugin Development

BookAtrium supports independently developed third-party plugins through the public NuGet package **`BookAtrium.PluginContracts`** (Plugin API 2.0).

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.0.1" />
```

Normal plugin development does **not** require access to the private BookAtrium application source. Do not reference private BookAtrium projects. Third-party plugin projects should reference only `BookAtrium.PluginContracts`.

Guides: [`docs/plugins/sdk-2`](docs/plugins/sdk-2/).  
Reusable CI: [`.github/workflows/plugin-build.yml`](.github/workflows/plugin-build.yml).  
Third-party catalogue submissions: [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins).  
Official BookAtrium plugins: [`plugins/official/`](plugins/official/) and [`registries/official-plugins.json`](registries/official-plugins.json) (not submitted to the community catalogue).

Plugin developers may choose whether their plugins are:

- Free
- Paid
- Subscription-based
- Open source
- Proprietary
- Free with optional paid functionality

BookAtrium does not operate an official third-party plugin marketplace or directory.

Third-party developers are responsible for independently hosting, distributing, licensing, marketing, selling, updating, and supporting their plugins.

Plugin developers are also responsible for:

- Plugin compatibility
- User documentation
- Security
- Privacy
- Data processing
- Payment arrangements
- Taxes
- Refunds
- Customer support
- Legal compliance
- Third-party intellectual-property rights
- Third-party software licence compliance

A plugin must not imply that it is developed, approved, certified, endorsed, sold, or supported by BookAtrium unless written permission has been granted.

Plugin developers must not use BookAtrium branding in a way that could mislead users into believing that an independent plugin is an official BookAtrium product.

The Plugin API package (`BookAtrium.PluginContracts`), guides under `docs/plugins/sdk-2/`, packaging format, compatibility rules, and permission model are published from this repository. Official first-party plugin catalogue metadata is also published here. Community catalogue listing for independently published plugins is separate and curated.

## Private Core Application Development

The BookAtrium core application is privately developed and proprietary.

Developers interested in helping with core application development may apply to become approved volunteer contributors.

Applications are reviewed individually. Submission of an application does not guarantee acceptance or access to the private repository.

Selection may depend on:

- Relevant technical experience
- Demonstrated quality of work
- Project requirements
- Areas where help is needed
- Availability and reliability
- Communication skills
- Security considerations
- Third-party component licensing
- Willingness to follow project procedures

## Core Developer Application

Developers may submit an initial application using the public Developer Contribution Interest form:

[Apply to assist with BookAtrium development](../../issues/new?template=05-developer-interest.yml)

The public application form must not contain:

- Identity documents
- Residential addresses
- Telephone numbers
- Passwords
- Licence keys
- Private source code
- Employer-confidential information
- Client information
- Signing certificates
- API credentials
- Other sensitive personal information

Only public-safe professional information should be included.

Suitable applicants may be invited to continue the application process privately.

## Conditions for Approved Core Contributors

Before receiving access to private source code or confidential project material, an approved contributor may be required to:

- Sign a non-disclosure agreement
- Sign an intellectual-property assignment or contribution agreement
- Confirm that contributions are voluntary and unpaid
- Accept repository access and security rules
- Accept coding, testing, documentation, and review standards
- Comply with applicable third-party component licences
- Protect all credentials and confidential material
- Use approved branches and pull-request workflows
- Avoid introducing copied or unauthorised third-party code
- Promptly report security incidents
- Return or permanently delete confidential material when access ends

An NDA protects confidential information, but a separate contribution or intellectual-property agreement may also be required to define ownership and permitted use of contributed work.

## Unpaid Contribution Basis

Core development participation is currently voluntary and unpaid.

Participation does not create:

- Employment
- A contractor relationship
- A partnership
- An agency relationship
- An entitlement to payment
- An entitlement to revenue sharing
- An entitlement to ownership of BookAtrium
- A guarantee of future work

BookAtrium may remain free, introduce optional premium features, offer paid services, create commercial editions, or adopt another commercial model in the future.

Approved contributors must accept that contributions may continue to be used, modified, distributed, licensed, and commercialised by the BookAtrium project in accordance with the signed contribution agreement.

## Contributor Recognition

Approved contributors whose work is accepted may receive public recognition.

Recognition may include:

- Listing as an assistant developer
- Listing as a project contributor
- Acknowledgement in release notes
- Acknowledgement in project documentation
- Credit for a specific feature or area

Recognition does not create ownership, payment rights, employment rights, or control over the project.

Recognition may be adjusted where necessary to reflect the contributor's role, period of involvement, accepted work, or later circumstances.

## Private Repository Access

Private repository access is granted only where needed and may be limited by role or area.

Access may be suspended or withdrawn at any time for reasons including:

- Completion of assigned work
- Inactivity
- Security concerns
- Breach of an agreement
- Failure to follow project standards
- Licensing limitations
- Project restructuring
- Conduct concerns

Loss of access does not remove continuing confidentiality, intellectual-property, or security obligations.

## Third-Party Components

BookAtrium may use commercial or separately licensed third-party components, including user-interface libraries.

Approved contributors must comply with all licence restrictions applicable to those components.

A contributor may need an appropriate licence, account, development entitlement, or other approval before working with certain parts of the application.

No contributor may copy, expose, share, publish, or misuse:

- Commercial component licence keys
- Signing certificates
- Private package credentials
- API keys
- Private dependencies
- Proprietary third-party files

## Pull Requests

Pull requests in this public repository are limited to approved collaborators.

The public repository may accept:

- Documentation corrections
- Repository configuration changes
- Public SDK changes when the SDK becomes available
- Sample plugin improvements
- Other approved public-facing contributions

Private core source-code changes must not be submitted through this public repository.

Approved core developers will use the private development repository and its required branch, review, and testing process.

## Code and Intellectual Property

Do not submit code, documentation, media, or other material unless you have the legal right to contribute it.

Contributions must not include:

- Copied proprietary code
- Code taken from another project without permission
- Material that conflicts with its original licence
- Confidential employer or client information
- Malware or hidden behaviour
- Unauthorised trademarks or copyrighted assets
- Credentials or secrets

Contributors must identify any third-party code or assets included in a submission and provide the applicable licence information.

## Privacy and Sensitive Information

Never post:

- Passwords
- Licence keys
- Private ebooks
- Personal documents
- Customer information
- Confidential business information
- Database backups
- Private source code
- Unedited logs containing sensitive data
- API credentials
- Signing certificates
- Private repository links
- Confidential agreements

Public issues, discussions, and comments can be viewed by anyone.

## Expected Conduct

All participants must follow the [BookAtrium Code of Conduct](CODE_OF_CONDUCT.md).

Please remain respectful, constructive, and honest.

Content containing harassment, abuse, spam, private information, malicious files, or deliberate misinformation may be edited, hidden, locked, or removed.

## Legal Agreements

The specific NDA, contribution agreement, plugin terms, software licence, and other legal documents will be published or provided when appropriate.

Nothing in this file replaces a signed agreement.

Where a signed agreement conflicts with this general guidance, the signed agreement will govern.
