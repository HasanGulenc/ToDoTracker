import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../auth/AuthContext';
import * as tasksApi from '../api/tasks';
import type { TaskResponse, TaskStatus, TaskPriority } from '../api/types';
import { TaskForm } from './TaskForm';
import { StatusBadge, PriorityBadge } from '../components/Badges';
import { format } from 'date-fns';

type SortBy = 'dueDate' | 'priority' | 'createdAt';
type SortDir = 'asc' | 'desc';

const PAGE_SIZE = 20;

function formatDate(dateStr: string): string {
  const [year, month, day] = dateStr.split('-').map(Number);
  return format(new Date(year, month - 1, day), 'MMM d, yyyy');
}

export function TasksPage() {
  const { auth } = useAuth();
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [filterStatus, setFilterStatus] = useState<TaskStatus | ''>('');
  const [filterPriority, setFilterPriority] = useState<TaskPriority | ''>('');
  const [sortBy, setSortBy] = useState<SortBy>('createdAt');
  const [sortDir, setSortDir] = useState<SortDir>('desc');

  const [formOpen, setFormOpen] = useState(false);
  const [editTask, setEditTask] = useState<TaskResponse | undefined>();

  const load = useCallback(async () => {
    if (!auth) return;
    setLoading(true);
    setError(null);
    try {
      const res = await tasksApi.getTasks(
        {
          ...(filterStatus ? { status: filterStatus } : {}),
          ...(filterPriority ? { priority: filterPriority } : {}),
          sortBy,
          sortDir,
          page,
          pageSize: PAGE_SIZE,
        },
        auth.token
      );
      setTasks(res.items);
      setTotal(res.total);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tasks');
    } finally {
      setLoading(false);
    }
  }, [auth, filterStatus, filterPriority, sortBy, sortDir, page]);

  useEffect(() => {
    load();
  }, [load]);

  function openCreate() {
    setEditTask(undefined);
    setFormOpen(true);
  }

  function openEdit(task: TaskResponse) {
    setEditTask(task);
    setFormOpen(true);
  }

  async function handleDelete(task: TaskResponse) {
    if (!auth) return;
    if (!window.confirm(`Delete "${task.title}"?`)) return;
    try {
      await tasksApi.deleteTask(task.id, auth.token);
      load();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Delete failed');
    }
  }

  function handleSaved() {
    setFormOpen(false);
    setPage(1);
    load();
  }

  function handleSortChange(value: string) {
    const [sb, sd] = value.split(':') as [SortBy, SortDir];
    setSortBy(sb);
    setSortDir(sd);
    setPage(1);
  }

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div className="page">
      <div className="page-header">
        <h2>
          All Tasks <span className="count">({total})</span>
        </h2>
        <button className="btn-primary" onClick={openCreate}>
          + New Task
        </button>
      </div>

      <div className="filters">
        <select
          value={filterStatus}
          onChange={e => {
            setFilterStatus(e.target.value as TaskStatus | '');
            setPage(1);
          }}
        >
          <option value="">All Statuses</option>
          <option value="Todo">Todo</option>
          <option value="InProgress">In Progress</option>
          <option value="Done">Done</option>
        </select>
        <select
          value={filterPriority}
          onChange={e => {
            setFilterPriority(e.target.value as TaskPriority | '');
            setPage(1);
          }}
        >
          <option value="">All Priorities</option>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
        <select value={`${sortBy}:${sortDir}`} onChange={e => handleSortChange(e.target.value)}>
          <option value="createdAt:desc">Newest first</option>
          <option value="createdAt:asc">Oldest first</option>
          <option value="dueDate:asc">Due date (earliest)</option>
          <option value="dueDate:desc">Due date (latest)</option>
          <option value="priority:desc">Priority (high → low)</option>
          <option value="priority:asc">Priority (low → high)</option>
        </select>
      </div>

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="loading">Loading…</div>
      ) : tasks.length === 0 ? (
        <div className="empty">No tasks found.</div>
      ) : (
        <table className="task-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Due Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {tasks.map(t => (
              <tr key={t.id}>
                <td>
                  <div className="task-title">{t.title}</div>
                  {t.description && <div className="task-desc">{t.description}</div>}
                </td>
                <td>
                  <StatusBadge status={t.status} />
                </td>
                <td>
                  <PriorityBadge priority={t.priority} />
                </td>
                <td>{t.dueDate ? formatDate(t.dueDate) : '—'}</td>
                <td>
                  <button className="btn-link" onClick={() => openEdit(t)}>
                    Edit
                  </button>
                  <button className="btn-link danger" onClick={() => handleDelete(t)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {!loading && totalPages > 1 && (
        <div className="pagination">
          <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}>
            ← Prev
          </button>
          <span>
            Page {page} of {totalPages}
          </span>
          <button
            onClick={() => setPage(p => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
          >
            Next →
          </button>
        </div>
      )}

      {formOpen && (
        <TaskForm task={editTask} onSaved={handleSaved} onClose={() => setFormOpen(false)} />
      )}
    </div>
  );
}
