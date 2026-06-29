# TODO - Fix 400 on /api/auth/register-company

- [x] Locate backend endpoint: AuthController.RegisterCompany -> POST api/auth/register-company
- [x] Identify server-side validation/business exceptions that map to 400 via GlobalExceptionMiddleware
- [ ] Fix DTO/backend mismatch causing 400 (likely JSON property names/validation)
- [ ] Update frontend request payload if needed
- [ ] Add detailed validation error response for model-binding failures (optional but recommended)
- [ ] Run backend and test endpoint with a known-good JSON payload

