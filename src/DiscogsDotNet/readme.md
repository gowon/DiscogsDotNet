# DiscogsDotNet Class Library

To-do:

- [ ] Create response objects for all endpoints
  - <https://www.discogs.com/developers/#>
  - <https://swagger-toolbox.firebaseapp.com/>
  - <https://roger13.github.io/SwagDefGen/>
  - <https://gist.github.com/0xdevalias/5fecf0db3bd9cc7465e42616061e1ab0>

## API Checklist

- [ ] Authentication
  - [x] Discogs Auth Flow
  - [ ] OAuth Flow
  - [ ] Request Token URL
  - [ ] Access Token URL
- [ ] Database
  - [ ] Release
  - [ ] Release Rating By User
  - [ ] Get Release Rating By User
  - [ ] Update Release Rating By User
  - [ ] Delete Release Rating By User
  - [ ] Community Release Rating
  - [ ] Release Stats
  - [ ] Master Release
  - [ ] Master Release Versions
  - [ ] Artist
  - [ ] Artist Releases
  - [ ] Label
  - [ ] All Label Releases
  - [ ] Search
  - [ ] Videos
- [ ] Marketplace
  - [ ] Inventory
  - [ ] Listing
  - [ ] Get listing
  - [ ] Edit A Listing
  - [ ] Delete A Listing
  - [ ] New Listing
  - [ ] Order
  - [ ] Get Order
  - [ ] Edit An Order
  - [ ] List Orders
  - [ ] List Order Messages
  - [ ] List Order Messages
  - [ ] Add New Message
  - [ ] Fee
  - [ ] Fee with currency
  - [x] Price Suggestions
  - [x] Release Statistics
- [ ] Inventory Export
  - [ ] Export your inventory
  - [ ] Get recent exports
  - [ ] Get an export
  - [ ] Download an export
- [ ] Inventory Upload
  - [ ] Add inventory
  - [ ] Change inventory
  - [ ] Delete inventory
  - [ ] Get recent uploads
  - [ ] Get an upload
- [ ] User Identity
  - [ ] Identity
  - [ ] Profile
  - [ ] Get Profile
  - [ ] Edit Profile
  - [ ] User Submissions
  - [ ] User Contributions
- [ ] User Collection
- [ ] Collection
  - [ ] Get Collection Folders
  - [ ] Create Folder
  - [ ] Collection Folder
  - [ ] Get Folders
  - [ ] Edit Folder
  - [ ] Delete Folder
  - [ ] Collection Items By Release
  - [ ] Collection Items By Folder
  - [ ] Add To Collection Folder
  - [ ] Change Rating Of Release
  - [ ] Delete Instance From Folder
  - [ ] List Custom Fields
  - [ ] Edit Fields Instance
  - [ ] Collection Value
- [ ] User Wantlist
  - [ ] Wantlist
  - [ ] Add To Wantlist
  - [ ] Add To Wantlist
  - [ ] Edit Release In Wantlist
  - [ ] Delete Release From Wantlist
- [ ] User Lists
  - [ ] User Lists
  - [ ] List

## Issues

Problem:

Postman to YAML conversion tool creates empty objects. NSwag generates "void" methods instead of "object" methods. This feels like a bug and not an expected result.

References:

- <https://github.com/RicoSuter/NSwag/issues/3912>

Solution:
After converting Postman collection to OAS YAML using tool, perform Regex to replace:

```yaml
            application/json: {}
```

with:

```taml
            application/json:
              schema:
                type: object
```

also:

```json
						"header": [
							{
								"key": "Content-Type",
								"value": "application/x-www-form-urlencoded",
								"type": "text"
							},
							{
								"key": "User-Agent",
								"value": "{{user_agent}}",
								"type": "text"
							}
						],
```

with:

```json
						"header": [],
```
