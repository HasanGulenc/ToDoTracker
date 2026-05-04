import { apiFetch } from './client';
import type {
  TaskResponse,
  TaskListResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  TaskListParams,
} from './types';

function buildQuery(params: Record<string, unknown>): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== '') {
      q.set(k, String(v));
    }
  }
  const s = q.toString();
  return s ? `?${s}` : '';
}

export function getTasks(params: TaskListParams, token: string): Promise<TaskListResponse> {
  return apiFetch<TaskListResponse>(`/api/tasks${buildQuery(params as Record<string, unknown>)}`, {}, token);
}

export function getDueToday(token: string): Promise<TaskResponse[]> {
  return apiFetch<TaskResponse[]>('/api/tasks/due-today', {}, token);
}

export function createTask(req: CreateTaskRequest, token: string): Promise<TaskResponse> {
  return apiFetch<TaskResponse>('/api/tasks', { method: 'POST', body: JSON.stringify(req) }, token);
}

export function updateTask(id: number, req: UpdateTaskRequest, token: string): Promise<TaskResponse> {
  return apiFetch<TaskResponse>(`/api/tasks/${id}`, { method: 'PUT', body: JSON.stringify(req) }, token);
}

export function deleteTask(id: number, token: string): Promise<void> {
  return apiFetch<void>(`/api/tasks/${id}`, { method: 'DELETE' }, token);
}
