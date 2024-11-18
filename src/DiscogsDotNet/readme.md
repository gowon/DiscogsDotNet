# DiscogsDotNet Class Library

To-do:

- [ ] Create response objects for all endpoints
  - <https://www.discogs.com/developers/#>
  - <https://swagger-toolbox.firebaseapp.com/>
  - <https://roger13.github.io/SwagDefGen/>
  - <https://gist.github.com/0xdevalias/5fecf0db3bd9cc7465e42616061e1ab0>

## Issues

Problem:

Postman to YAML conversion tool creates empty objects. NSwag generates "void" methods instead of "object" methods. This feels like a bug and not an expected result.

References:

- <https://github.com/RicoSuter/NSwag/issues/3912>

Solution:
After converting Postman collection to OAS YAML using tool, perform Regex to replace:

```text
            application/json: {}
```

with:

```text
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
