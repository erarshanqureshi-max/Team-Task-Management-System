import React from 'react';

export const StatusBadge = ({ status }) => {
  let badgeClass = 'bg-secondary';

  switch (status?.toLowerCase()) {
    case 'to do':
      badgeClass = 'bg-secondary';
      break;
    case 'in progress':
      badgeClass = 'bg-primary';
      break;
    case 'done':
      badgeClass = 'bg-success';
      break;
    default:
      badgeClass = 'bg-dark';
  }

  return (
    <span className={`badge ${badgeClass} text-uppercase px-2 py-1`} style={{ fontSize: '0.75rem', letterSpacing: '0.5px' }}>
      {status || 'Unknown'}
    </span>
  );
};
