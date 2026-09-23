import React from 'react';

export const PriorityBadge = ({ priority }) => {
  let badgeClass = 'bg-secondary';
  let icon = 'bi-flag';

  switch (priority?.toLowerCase()) {
    case 'low':
      badgeClass = 'bg-info text-dark';
      icon = 'bi-arrow-down';
      break;
    case 'medium':
      badgeClass = 'bg-primary';
      icon = 'bi-arrow-right';
      break;
    case 'high':
      badgeClass = 'bg-warning text-dark';
      icon = 'bi-arrow-up';
      break;
    case 'urgent':
      badgeClass = 'bg-danger';
      icon = 'bi-exclamation-triangle';
      break;
    default:
      badgeClass = 'bg-secondary';
  }

  return (
    <span className={`badge ${badgeClass} px-2 py-1 align-items-center d-inline-flex gap-1`} style={{ fontSize: '0.75rem' }}>
      <i className={`bi ${icon}`}></i>
      <span>{priority || 'Normal'}</span>
    </span>
  );
};
