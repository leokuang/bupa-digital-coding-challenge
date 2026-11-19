import type { CategorizedBooksResponse } from '../types/book';


export async function fetchCategorizedBooks(
  hardcoverOnly: boolean,
  signal?: AbortSignal
): Promise<CategorizedBooksResponse> {
  const response = await fetch(`/api/books?hardcoverOnly=${hardcoverOnly}`, {
    signal,
    headers: { Accept: 'application/json' },
  });

  if (!response.ok) {
    const errorBody = await response.json().catch(() => null);
    throw new Error(
      errorBody?.message || `Request failed with status ${response.status}`
    );
  }

  return (await response.json()) as CategorizedBooksResponse;
}
