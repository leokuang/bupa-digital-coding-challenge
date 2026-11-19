import { render, screen, fireEvent, waitFor, cleanup } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import App from './App';
import * as booksApi from './api/booksApi';

describe('App', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    cleanup();
  });

  it('fetches and displays categorized books when Get Books is clicked', async () => {
    vi.spyOn(booksApi, 'fetchCategorizedBooks').mockResolvedValue({
      adults: ['Great Expectations', 'Hamlet'],
      children: ['The Hobbit'],
    });

    render(<App />);
    fireEvent.click(screen.getByRole('button', { name: /get books/i }));

    await waitFor(() => {
      expect(screen.getByText('Books owned by Adults')).toBeInTheDocument();
      expect(screen.getByText('Books owned by Children')).toBeInTheDocument();
      expect(screen.getByText('Great Expectations')).toBeInTheDocument();
      expect(screen.getByText('The Hobbit')).toBeInTheDocument();
    });
  });

  it('requests and displays only hardcover books when the filter is toggled', async () => {
    const fetchBooks = vi
      .spyOn(booksApi, 'fetchCategorizedBooks')
      .mockResolvedValueOnce({
        adults: ['Paperback Novel', 'Hardcover Hamlet'],
        children: ['Paperback Story', 'Hardcover Hobbit'],
      })
      .mockResolvedValueOnce({
        adults: ['Hardcover Hamlet'],
        children: ['Hardcover Hobbit'],
      });

    render(<App />);
    fireEvent.click(screen.getByRole('button', { name: /get books/i }));

    await waitFor(() => {
      expect(screen.getByText('Paperback Novel')).toBeInTheDocument();
      expect(screen.getByText('Paperback Story')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByLabelText(/hardcover only/i));

    await waitFor(() => {
      expect(screen.getByText('Hardcover Books owned by Adults')).toBeInTheDocument();
      expect(screen.getByText('Hardcover Books owned by Children')).toBeInTheDocument();
      expect(screen.getByText('Hardcover Hamlet')).toBeInTheDocument();
      expect(screen.getByText('Hardcover Hobbit')).toBeInTheDocument();
      expect(screen.queryByText('Paperback Novel')).not.toBeInTheDocument();
      expect(screen.queryByText('Paperback Story')).not.toBeInTheDocument();
      expect(fetchBooks).toHaveBeenNthCalledWith(2, true, expect.any(AbortSignal));
    });
  });

  it('clears stale results and reports an error when a refresh fails', async () => {
    vi.spyOn(booksApi, 'fetchCategorizedBooks')
      .mockResolvedValueOnce({ adults: ['Previously loaded book'], children: [] })
      .mockRejectedValueOnce(new Error('Unable to retrieve books.'));

    render(<App />);
    fireEvent.click(screen.getByRole('button', { name: /get books/i }));

    await waitFor(() => {
      expect(screen.getByText('Previously loaded book')).toBeInTheDocument();
    });

    fireEvent.click(screen.getByLabelText(/hardcover only/i));

    await waitFor(() => {
      expect(screen.queryByText('Previously loaded book')).not.toBeInTheDocument();
      expect(screen.getByRole('alert')).toHaveTextContent('Unable to retrieve books.');
    });
  });
});
