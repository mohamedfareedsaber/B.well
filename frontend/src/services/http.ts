export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5068';

export type ApiResponse<T> = {
  success: boolean;
  message?: string | null;
  data?: T;
  errors?: string[] | null;
};

export async function apiFetch<T>(
  path: string,
  options: RequestInit & { token?: string | null } = {},
): Promise<T> {
  const { token, headers, ...rest } = options;
  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...rest,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
  });

  const json = (await res.json().catch(() => null)) as ApiResponse<T> | null;

  if (!res.ok || !json?.success) {
    const message = json?.message || json?.errors?.join(', ') || `Request failed (${res.status})`;
    throw new Error(message);
  }

  return json.data as T;
}
