# Image Generation Prompts - Spec-Driven API Modernization

## Thumbnail Prompt

**Use the following prompt across all image generators to create a thumbnail for this lab:**

### Prompt (for all generators):

> A professional tech illustration showing API modernization - on the left, old SOAP/WSDL envelopes and XML documents; on the right, modern OpenAPI/REST represented as clean JSON flowing through an API gateway. Include a specification document as the bridge between old and new. Blue and green color palette with API endpoint visuals. 16:9 aspect ratio, clean modern design suitable as a repository thumbnail.

### Settings:
- **Aspect Ratio:** 16:9 (landscape)
- **Resolution:** 1792x1024 or similar
- **Style:** Professional tech illustration, clean, modern

### Generators to use:
1. **Google Gemini Pro** (Imagen 3)
2. **Azure OpenAI GPT-Image-2** (via Azure AI Foundry)
3. **Microsoft Image Creator** (Bing/Designer)

### Output:
Save generated images to:
- `assets/thumbnail-gemini.png`
- `assets/thumbnail-gpt-image.png`
- `assets/thumbnail-msdesigner.png`

After selecting the best one, rename to `assets/thumbnail.png` and update the `thumbnail` field in `appmodlab.md`.
