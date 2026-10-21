/** Extract a human-readable message from RTK Query / fetch errors. */
export function getErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (!error) return fallback;
  if (typeof error === 'string') return error;
  if (error instanceof Error) return error.message;

  if (typeof error === 'object') {
    const e = error as {
      data?: { message?: string; errors?: string[]; title?: string };
      error?: string;
      message?: string;
      status?: number | string;
    };

    if (e.data?.message) return e.data.message;
    if (e.data?.errors?.length) return e.data.errors.join(' ');
    if (e.data?.title) return e.data.title;
    if (typeof e.error === 'string' && e.error !== 'Rejected') return e.error;
    if (typeof e.message === 'string') return e.message;
    if (e.status === 'FETCH_ERROR') return 'Cannot reach the API. Is the backend running?';
  }

  return fallback;
}
