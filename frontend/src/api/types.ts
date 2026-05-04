export type TaskStatus = 'Todo' | 'InProgress' | 'Done';
export type TaskPriority = 'Low' | 'Medium' | 'High';

export interface TaskResponse {
  id: number;
  title: string;
  description: string | null;
  dueDate: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  createdAt: string;
  updatedAt: string;
}

export interface TaskListResponse {
  items: TaskResponse[];
  total: number;
  page: number;
  pageSize: number;
}

export interface AuthResponse {
  token: string;
  email: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string;
  status: TaskStatus;
  priority: TaskPriority;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string;
  status: TaskStatus;
  priority: TaskPriority;
}

export interface TaskListParams {
  status?: TaskStatus;
  priority?: TaskPriority;
  sortBy?: 'dueDate' | 'priority' | 'createdAt';
  sortDir?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}
