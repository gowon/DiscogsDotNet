# DiscogsDotNet

## Development

Discogs regretably does not provide an OpenAPI spec for their API, and lacks any real definition for response objects. An OAS spec would be ideal as tooling could generate clients in several languages, not just C#. A definitive community-driven OAS spec does not exist. Given the maturity of the API, once an OAS file has been created, it should be minimal to maintain.

### Sources

- <https://github.com/leopuleo/Discogs-Postman> contains Postman Collection v2.1/v2.0 Collections covering the Discogs API.
- <https://github.com/buntine/discogs> contains [samples](https://github.com/buntine/discogs/tree/e2a600ee451eb00b4cef2b38adb7c645cda274d4/spec/samples) of the response objects for (all?) endpoints.

### OpenAPI Spec Methodology

1. Download and extract source assets to `WORKDIR`:

    ```powershell
    curl -fsSLO https://github.com/leopuleo/Discogs-Postman/archive/refs/heads/master.zip
    curl -fsSLO https://github.com/buntine/discogs/archive/refs/heads/master.zip
    ```

    > [!NOTE]
    > Make as many corrections to the source Postman script to clean up generated output

2. Generate OpenAPI 3.x YAML spec from  Postman Collection using [postman-to-openapi CLI tool](https://github.com/joolfe/postman-to-openapi):

    ```powershell
    npx p2o .\Discogs.postman_collection-v2.1.json -f openapi.yaml -o .\p2o.options.json
    ```

3. Generate OpenAPI Definitions from sample JSON responses using [mock-to-openapi CLI tool](https://github.com/OzzyCzech/mock-to-openapi)

    ```powershell
    npx mock-to-openapi . .\yaml -v -w
    ```

4. Combine all YAML into single spec file.

5. Generate C# API Client code from OpenAPI 3.x YAML using NSwag:

    ```powershell
    nswag run .\src\DiscogsDotNet\nswag.json
    ```

6. Add authorization and rate limiting support to generated client library

References:

- <https://gist.github.com/0xdevalias/5fecf0db3bd9cc7465e42616061e1ab0>

### Collection Value Spreadsheet Methodology

1. Search User's custom fields for the ones that represent Media Condition and Sleeve Condition (id)
2. Get all (paged) releases in collection <https://www.discogs.com/developers?srsltid=AfmBOoo1A54MFg0JLNBIXLaH_hCZhixUdiI_jzJDLUxltTZBBk292hkW#page:user-collection,header:user-collection-collection-items-by-folder>. (folder_id =1, per_page=100)
3. For each release (id, resource_url, artists:0:name, basic_information:title, year):
   1. Get Price Suggestion <https://www.discogs.com/developers?srsltid=AfmBOoo1A54MFg0JLNBIXLaH_hCZhixUdiI_jzJDLUxltTZBBk292hkW#page:marketplace,header:marketplace-price-suggestions>