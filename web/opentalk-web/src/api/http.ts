export const API_BASE: string =
  (import.meta as any).env?.VITE_API_BASE ||
  localStorage.getItem('API_BASE') ||
  'https://localhost:49188'

export async function http<T = any>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(API_BASE + path, init)
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  // @ts-ignore
  return (await res.json()) as T
}

// Auth
export function login(username: string, password: string) {
  return http('/api/v1/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  })
}

// Groups
export function getGroups(userId: number) {
  return http(`/api/v1/groups/${userId}`)
}

export function getGroupMembers(groupId: number) {
  return http(`/api/v1/groups/${groupId}/members`)
}

// Messages
export function getMessages(groupId: number, limit = 50) {
  return http(`/api/v1/messages/${groupId}?limit=${limit}`)
}

export function getDirectMessages(userId: number, peerId: number, limit = 50) {
  return http(`/api/v1/messages/direct/${userId}/${peerId}?limit=${limit}`)
}

// Files
export async function uploadFile(file: File) {
  const form = new FormData()
  form.append('file', file)
  return await http('/api/v1/files/upload', { method: 'POST', body: form })
}
