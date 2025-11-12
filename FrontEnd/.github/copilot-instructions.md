# BookManager Frontend - AI Coding Agent Instructions

## Project Overview

Angular 16 SPA for managing a book catalog with authors, subjects, and pricing. Backend API at `http://localhost:5000/api` (configurable in `src/app/config/api.config.ts`). Uses Bootstrap 5 for styling and pt-BR locale.

## Architecture Patterns

### Component Structure

- **List components**: Read-only views with CRUD navigation (`livros-list`, `autores`, `assuntos`)
- **Form components**: Dual-purpose - create/edit via route detection OR read-only detail view
  - Route `/livros/:id` → read-only mode (form disabled)
  - Route `/livros/editar/:id` → edit mode
  - Route `/livros/novo` → create mode
  - See `livro-form.component.ts` lines 40-51 for routing logic

### Data Flow

1. Services (`src/app/services/*.service.ts`) use `HttpClient` with typed DTOs
2. All services are `providedIn: 'root'` (singleton pattern)
3. API responses typed via `src/app/models/models.ts` DTOs
4. Components subscribe to observables, handle `next` and `error` callbacks
5. Error handling pattern: display user message + log to console

### Forms & Validation

- **Reactive Forms** (FormBuilder) used for complex forms (e.g., `livro-form`)
- **Template-driven** forms for simple CRUD (e.g., `assuntos`, `autores`)
- Custom validators in `livro-form.component.ts`:
  - `requiredTrimValidator`: Ensures non-empty after trimming
  - `minLengthArray`: Validates FormArray minimum length
  - ISBN pattern: `/^(?:\d{10}|\d{13}|(?:\d[ -]?){9}[\dxX]|(?:\d[ -]?){13})$/`

### Backend Error Handling (Critical Pattern)

The `livro-form` component implements sophisticated server error mapping (lines 169-224):

```typescript
// Parses ASP.NET error formats: string, array, or object with errors/erros
parseBackendErrors(err) → { mensagem, mensagens }
// Maps error messages to form controls by keywords
applyServerErrorsToControls(mensagens) → setErrors on controls
// Handles UNIQUE constraint violations for ISBN duplicates
```

**When creating/editing forms, replicate this error handling pattern for production-ready UX.**

### Multi-Value Relationships

Books have many-to-many relationships (Autores, Preços). Pattern in `livro-form`:

- Use `FormArray` for dynamic fields (`precos` array)
- Load related entities into component arrays (`autores[]`, `formasPagamento[]`)
- Submit as ID arrays (`idAutores: number[]`) or nested DTOs (`precos: CreateLivroPrecoDto[]`)
- See lines 59-69 for FormArray structure, 145-150 for add/remove methods

### Reports Service

Generates PDF/Excel/Word via backend (`relatorios.service.ts`):

- Returns `Blob` with `responseType: 'blob'`
- Helper methods: `downloadArquivo()` creates temporary URL for download
- Query params built with `HttpParams` (avoid sending null/undefined - check before adding)
- File naming: `obterNomeArquivo(nomeRelatorio, formato)` with ISO timestamp

## Key Conventions

### TypeScript

- **Strict null checks**: Use `?` for optional properties in interfaces
- **Non-null assertion** (`!`) for FormGroup/FormArray initialized in `ngOnInit`
- **Type guards**: Check `Array.isArray()` before operations (see error parsing)

### Angular Specifics

- Prefix: `app` (configured in `angular.json`)
- Locale: `pt-BR` registered in `app.module.ts` (affects currency/date pipes)
- Bootstrap CSS imported globally in `angular.json` styles array

### File Organization

```
components/
  feature/
    feature.component.ts
    feature.component.html
    feature.component.css
services/
  feature.service.ts
  feature.service.spec.ts  # Jasmine tests
models/
  models.ts               # Shared DTOs
  feature.models.ts       # Feature-specific types
```

## Development Workflow

### Local Development

```bash
npm start                    # Starts dev server on http://localhost:4200
npm run build               # Production build → dist/book-manager-frontend
npm test                    # Runs Jasmine/Karma tests
npm run watch               # Watch mode for development
```

### Docker Deployment

- **Multi-stage build**: Node 18 Alpine → Nginx 1.25 Alpine
- Build output: `dist/book-manager-frontend` → `/usr/share/nginx/html`
- Nginx config includes SPA fallback (`try_files $uri $uri/ /index.html`)
- Gzip enabled for text assets, 1-year cache for static files
- **Important**: Use `npm install --force` in Dockerfile (peer dependency conflicts)

### API Integration

- Base URL in `src/app/config/api.config.ts` (default: `http://localhost:5000/api`)
- All endpoints follow pattern: `${apiUrl}/EntityPlural` (e.g., `/api/Livros`, `/api/Autores`)
- Standard REST verbs: GET (all, active, by ID), POST, PUT, DELETE

## Testing Considerations

- Unit tests use Jasmine (`*.spec.ts` files)
- Services inject `HttpClient` → mock with `HttpClientTestingModule`
- Forms: Test validation logic, error mapping, and FormArray operations
- No E2E framework configured (add Playwright/Cypress if needed)

## Common Tasks

### Adding a New Entity

1. Define DTOs in `src/app/models/models.ts` (e.g., `EntityDto`, `CreateEntityDto`, `UpdateEntityDto`)
2. Create service in `src/app/services/entity.service.ts` with standard CRUD methods
3. Create list component (template-driven if simple, reactive if complex)
4. Create form component (follow `livro-form` pattern for error handling)
5. Add routes in `app-routing.module.ts`
6. Register components in `app.module.ts` declarations

### Handling Complex Forms

- Use `FormArray` for dynamic repeating fields
- Implement route-based mode detection (view/edit/create)
- Add server error mapping with keyword matching
- Disable form in read-only mode: `this.form.disable({ emitEvent: false })`

### Working with Reports

- Add params interface in `relatorios.models.ts`
- Create service method returning `Observable<Blob>`
- Build `HttpParams` conditionally (skip null/undefined)
- Use `downloadArquivo()` helper for browser download

## Gotchas

- **FormArray min length**: Use custom validator `minLengthArray(n)` not built-in `Validators.minLength`
- **Currency inputs**: Format on blur but store as number (see `formatValor()` in livro-form)
- **Route params**: Convert to number with `+id` (paramMap returns strings)
- **Locale pipes**: Require `registerLocaleData(localePt, 'pt-BR')` for Brazilian formatting
- **Docker npm install**: Must use `--force` flag due to Angular 16 peer dependencies
