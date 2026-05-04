import type { TaskStatus, TaskPriority } from '../api/types';

const STATUS_CLASS: Record<TaskStatus, string> = {
  Todo: 'badge-todo',
  InProgress: 'badge-inprogress',
  Done: 'badge-done',
};

const STATUS_LABEL: Record<TaskStatus, string> = {
  Todo: 'Todo',
  InProgress: 'In Progress',
  Done: 'Done',
};

const PRIORITY_CLASS: Record<TaskPriority, string> = {
  Low: 'badge-low',
  Medium: 'badge-medium',
  High: 'badge-high',
};

export function StatusBadge({ status }: { status: TaskStatus }) {
  return <span className={`badge ${STATUS_CLASS[status]}`}>{STATUS_LABEL[status]}</span>;
}

export function PriorityBadge({ priority }: { priority: TaskPriority }) {
  return <span className={`badge ${PRIORITY_CLASS[priority]}`}>{priority}</span>;
}
