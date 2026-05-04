import { useState, useEffect } from 'react';
import { useAuth } from '../auth/AuthContext';
import * as tasksApi from '../api/tasks';
import type { TaskResponse } from '../api/types';
import { StatusBadge, PriorityBadge } from '../components/Badges';
import { format } from 'date-fns';

export function DueTodayPage() {
  const { auth } = useAuth();
  const [tasks, setTasks] = useState<TaskResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!auth) return;
    setLoading(true);
    tasksApi
      .getDueToday(auth.token)
      .then(setTasks)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, [auth]);

  const today = format(new Date(), 'MMMM d, yyyy');

  return (
    <div className="page">
      <div className="page-header">
        <h2>
          Due Today <span className="count">— {today}</span>
        </h2>
      </div>

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="loading">Loading…</div>
      ) : tasks.length === 0 ? (
        <div className="empty">Nothing due today.</div>
      ) : (
        <table className="task-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Status</th>
              <th>Priority</th>
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
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
