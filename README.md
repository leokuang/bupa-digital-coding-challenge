# Bupa Digital Coding Challenge — Book Library

A full-stack enterprise web application built with **ASP.NET Core (C#)** and **React (TypeScript + Vite)** that retrieves book owner records from the Bupa Digital Coding Test API, categorises books by owner age group (**Adults 18+** vs **Children 17 and under**), supports dynamic **Hardcover-only** filtering with automatic heading adaptation, and displays titles sorted alphabetically.

---

## Important Requirements & Compliance

| Requirement | Implementation Details |
| :--- | :--- |
| **"When you click 'Get Books', all books are shown, grouped and sorted"** | Clicking **Get Books** fetches book owners from the API, categorises them into Adults (18+) and Children (17 and under), filters out non-hardcover books (when unchecked), and renders titles sorted alphabetically (`OrdinalIgnoreCase`). Headings dynamically display **"Books owned by Adults"** and **"Books owned by Children"**. |
| **"When you click 'Hardcover only', only hardcover books should be shown, and headings update"** | Toggling **Hardcover only** dynamically restricts results to books matching the `Hardcover` type (case-insensitively) and instantly updates the section headings to **"Hardcover Books owned by Adults"** and **"Hardcover Books owned by Children"**. |
| **"The blue colour used throughout the app is #0079c8"** | The primary brand blue (`#0079c8`) is used for the header, category headings, primary button, and checkbox accent. Hover and focus states use supporting shades for clear interaction feedback. |
| **"The mobile breakpoint is 600px"** | Responsive CSS Grid layout adapts seamlessly at the `600px` breakpoint, switching from side-by-side category columns to a stacked single-column layout. |
| **"Production Quality & Best Practices"** | The app includes robust error handling, server-side caching (`IMemoryCache`), typed HTTP requests, and cancellation of unfinished requests with `AbortController`. It also uses clearly defined TypeScript types, accessible status and error messages (`aria-live` and `role="alert"`), and focused backend and frontend tests using xUnit and Vitest. |

---

## Assumptions Made

1. **Age Boundary:** Owners with `Age >= 18` are classified as **Adults** and `Age <= 17` as **Children**.
2. **Book Type Matching:** Hardcover status is checked case-insensitively (`book.Type?.Trim() == "Hardcover"`). Books with null or mismatched types are excluded when the hardcover filter is enabled.
3. **Alphabetical Sorting:** Book titles within each category are sorted case-insensitively using ordinal comparison (`StringComparer.OrdinalIgnoreCase`), independent of the machine's locale.
4. **Duplicate Titles:** If multiple owners possess identical book titles, each instance is processed and listed according to their respective age bucket and filter state.
5. **Upstream Rate Limiting:** Successful, non-empty responses are cached in process for 30 seconds to reduce repeated upstream requests. A `429` on a cache miss is returned as a structured error; the cache does not eliminate rate limits on cold starts or after expiry.

---

## Project Codebase Structure

```text
├── BupaBooks.slnx                    # .NET Solution file
├── docker-compose.yml                # Docker compose configuration
├── backend/
│   ├── BupaBooks.Api/                # ASP.NET Core Web API
│   │   ├── Controllers/              # HTTP API Controllers (BooksController.cs)
│   │   ├── Services/                 # Business logic & external API clients (BupaBooksClient.cs, BookCategorizationService.cs)
│   │   ├── Models/                   # Data transfer objects & response contracts (BookModels.cs)
│   │   ├── Program.cs                # Dependency injection & pipeline configuration
│   │   └── appsettings.json          # Configuration settings
│   └── BupaBooks.Api.Tests/          # xUnit unit tests for domain logic & edge cases
│       ├── BookCategorizationServiceTests.cs
│       └── BooksApiBehaviorTests.cs
└── frontend/
    ├── src/
    │   ├── api/                      # Typed API client with AbortSignal support (booksApi.ts)
    │   ├── types/                    # TypeScript response contracts (book.ts)
    │   ├── assets/                   # Static icons & hero image
    │   ├── App.tsx                   # Main UI component & event-driven state
    │   ├── App.test.tsx              # Vitest + React Testing Library component tests
    │   ├── App.css                   # #0079c8 theme & responsive breakpoints
    │   └── main.tsx                  # React DOM entry point
    ├── vite.config.ts                # Vite server, API proxy, and jsdom test config
    └── package.json                  # Frontend dependencies & scripts
```

---

## Data Flow & Request Trace

### Data Ingress & Egress Points
* **Data In:** 
  1. External Bupa Book Owners API (`https://bupa.com.au/...`) fetched via `BupaBooksClient`.
  2. Browser User input (`Get Books` click / `Hardcover only` checkbox toggle) sent to `/api/books`.
* **Data Out:**
  1. Rendered JSON response (`CategorizedBooksResponse` containing sorted Adult and Child book title arrays) returned to the React frontend.
  2. Rendered UI components displaying categorized book lists with ARIA accessibility attributes.

### Request Flow Trace (Controllers to Database / Upstream Layer)

```text
 [ React Frontend ]
        │
        │ 1. HTTP GET /api/books?hardcoverOnly={bool}
        ▼
 [ BooksController ] (ASP.NET Core API Controller)
        │
        │ 2. Calls IBupaBooksClient.GetBookOwnersAsync()
        ▼
 [ BupaBooksClient ] (Infrastructure Layer)
        │
        ├── 3a. Check IMemoryCache (30-second TTL cache hit? Return cached book owners)
        │
        └── 3b. Cache Miss ──► 4. HTTP GET to Upstream Bupa API (External Source)
                                       │
                                       ▼ 5. Raw JSON response (External Book Owners & Books)
 [ BupaBooksClient ] ◄─────────────────┘
        │
        │ 6. Deserialises & Caches in IMemoryCache
        ▼
 [ BookCategorizationService ] (Domain & Business Logic Layer)
        │
        ├── 7a. Filters Owners by Age: Age >= 18 (Adults) vs Age <= 17 (Children)
        ├── 7b. Filters Books: Case-insensitive 'Hardcover' matching (if hardcoverOnly = true)
        ├── 7c. Defensive checks: skips null book collections and empty titles
        └── 7d. Alphabetically sorts book titles (case-insensitive OrdinalIgnoreCase)
        │
        ▼ 8. Returns CategorizedBooksResponse (Adults[] & Children[])
 [ BooksController ]
        │
        ▼ 9. HTTP 200 OK with JSON payload
 [ React Frontend ] (Displays Categorized Book Lists)
```

---

## Prerequisites

* **.NET SDK** (10.0, required by the backend target framework)
* **Node.js** (20.x LTS or newer) & **npm**

---

## How to Run Locally

Start the backend and frontend in two separate terminal sessions from the repository root:

### 1. Start the C# .NET Backend
```bash
dotnet run --project backend/BupaBooks.Api --launch-profile http
```
* The API listens on **`http://localhost:5015`**.
* Endpoints:
  * `GET http://localhost:5015/api/books?hardcoverOnly=false`
  * OpenAPI Spec: `http://localhost:5015/openapi/v1.json`

### 2. Start the React Frontend
```bash
cd frontend
npm install
npm run dev
```
* Open **`http://localhost:5173`** in your browser.

---

## Running Automated Tests & Linting

### Backend Unit Tests (xUnit)
Tests cover the `Age >= 18` vs `Age <= 17` boundary, alphabetical sorting, case-insensitive `Hardcover` filtering, defensive handling of `null` payloads, successful-response cache reuse, and mapping an upstream `429` to the documented API response:
```bash
dotnet test
```

### Frontend Component Tests & Linting (Vitest + ESLint)
Tests verify user interactions (`Get Books` click, the hardcover-filter request and returned results) and enforce strict TypeScript and React Hooks lint rules:
```bash
cd frontend
npm test
npm run lint
npm run build
```

---

## Future Improvements

1. **Support More Features:** Add search, pagination, and other book filters as the library grows.
2. **Scale for More Users:** Replace local memory caching with a shared cache, such as Redis, and add monitoring for API errors and slow requests.
3. **Improve Delivery:** Add CI/CD pipelines and deploy the frontend and backend as separate services to support frequent, automated releases.

---

## Challenges Encountered & How They Were Resolved

During integration with the live Bupa test endpoint (`https://bupa.com.au`), two production-relevant issues were identified and resolved:

### 1. Upstream Rate Limiting (`HTTP 429 Too Many Requests`)
* **Issue:** Repeated requests to the upstream Bupa endpoint (such as clicking *Get Books* multiple times or toggling the *Hardcover only* filter) triggered `HTTP 429 Too Many Requests` responses from the upstream server.
* **Fix:**
  1. Introduced server-side caching via **`IMemoryCache`** inside `BupaBooksClient`. Non-empty upstream payloads are cached for **30 seconds**, so repeated filter toggles and refreshes during that window reuse the cached owner data instead of making another upstream request. This reduces requests but does not guarantee avoidance of rate limits.
  2. Added explicit exception handling in `BooksController` for `HttpStatusCode.TooManyRequests`. If the upstream rate limit is hit on a cold cache or after expiry, the API returns a structured `429` JSON response with a clear, user-friendly message rather than an unhandled server error.

### 2. Intermittent Empty Response Bodies (`JsonException` at Byte 0)
* **Issue:** Under rate-limiting or transient upstream conditions, the external endpoint occasionally returned a `200 OK` status with a 0-byte empty body, causing `GetFromJsonAsync` to throw a `System.Text.Json.JsonException`.
* **Fix:** Updated `BupaBooksClient` to explicitly send `Accept: application/json` and `User-Agent` headers, read the raw response payload first, guard against empty/whitespace bodies before deserialising, and avoid caching empty responses.

### 3. Avoiding Cascading Renders in React 19
* **Issue:** Triggering API fetches inside a `useEffect` hook in response to filter state changes introduced extra render passes and triggered `react-hooks/set-state-in-effect` lint violations.
* **Fix:** Moved data fetching directly into explicit user event handlers (`handleGetBooks` and `handleHardcoverChange`) paired with `AbortController` via `useRef`, eliminating unnecessary effects while safely cancelling any in-flight requests.
