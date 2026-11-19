import { useState, useRef } from 'react';
import { fetchCategorizedBooks } from './api/booksApi';
import type { CategorizedBooksResponse } from './types/book';
import './App.css';

export function App() {
  const [books, setBooks] = useState<CategorizedBooksResponse | null>(null);
  const [hasRequested, setHasRequested] = useState(false);
  const [hardcoverOnly, setHardcoverOnly] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const abortControllerRef = useRef<AbortController | null>(null);

  const loadBooks = async (filterHardcover: boolean) => {
    abortControllerRef.current?.abort();
    const controller = new AbortController();
    abortControllerRef.current = controller;

    setIsLoading(true);
    setError(null);
    setBooks(null);

    try {
      const data = await fetchCategorizedBooks(filterHardcover, controller.signal);
      setBooks(data);
    } catch (err) {
      if (err instanceof Error && err.name === 'AbortError') return;
      setError(
        err instanceof Error
          ? err.message
          : 'Unable to retrieve books. Please try again.'
      );
    } finally {
      if (!controller.signal.aborted) {
        setIsLoading(false);
      }
    }
  };

  const handleGetBooks = () => {
    setHasRequested(true);
    void loadBooks(hardcoverOnly);
  };

  const handleHardcoverChange = (checked: boolean) => {
    setHardcoverOnly(checked);
    if (hasRequested) {
      void loadBooks(checked);
    }
  };

  const adultHeading = hardcoverOnly
    ? 'Hardcover Books owned by Adults'
    : 'Books owned by Adults';

  const childrenHeading = hardcoverOnly
    ? 'Hardcover Books owned by Children'
    : 'Books owned by Children';

  return (
    <div className="app-shell">
      <header className="bupa-header">
        <div className="container">
          <h1>Owners and Books</h1>
        </div>
      </header>

      <main className="container main-content">
        {error && (
          <div role="alert" className="error-banner">
            {error}
          </div>
        )}

        {books && (
          <section className="results-grid" aria-live="polite">
            <article className="category-card">
              <h2 className="category-heading">{adultHeading}</h2>
              {books.adults.length > 0 ? (
                <ul className="book-list">
                  {books.adults.map((title, index) => (
                    <li key={`adult-${title}-${index}`}>{title}</li>
                  ))}
                </ul>
              ) : (
                <p className="empty-state">No books found in this category.</p>
              )}
            </article>

            <article className="category-card">
              <h2 className="category-heading">{childrenHeading}</h2>
              {books.children.length > 0 ? (
                <ul className="book-list">
                  {books.children.map((title, index) => (
                    <li key={`child-${title}-${index}`}>{title}</li>
                  ))}
                </ul>
              ) : (
                <p className="empty-state">No books found in this category.</p>
              )}
            </article>
          </section>
        )}

        <section className="controls-bar" aria-label="Book filters and actions">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={hardcoverOnly}
              onChange={(e) => handleHardcoverChange(e.target.checked)}
            />
            <span>Hardcover only</span>
          </label>

          <button
            type="button"
            className="btn-primary"
            onClick={handleGetBooks}
            disabled={isLoading}
          >
            {isLoading ? 'Loading Books...' : 'Get Books'}
          </button>
        </section>
      </main>
    </div>
  );
}

export default App;

