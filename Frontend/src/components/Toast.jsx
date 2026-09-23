import React from 'react';

export const Toast = ({ message, type = 'success', onClose }) => {
  if (!message) return null;

  const bgClass = type === 'error' ? 'bg-danger text-white' : type === 'warning' ? 'bg-warning text-dark' : 'bg-success text-white';

  return (
    <div
      className={`alert alert-dismissible fade show position-fixed top-0 end-0 m-3 shadow-sm ${bgClass}`}
      style={{ zIndex: 1080, minWidth: '280px' }}
      role="alert"
    >
      <div className="d-flex align-items-center">
        <i className={`bi ${type === 'error' ? 'bi-x-circle' : 'bi-check-circle'} me-2 fs-5`}></i>
        <div>{message}</div>
        <button
          type="button"
          className="btn-close ms-auto"
          aria-label="Close"
          onClick={onClose}
          style={{ filter: type === 'warning' ? 'none' : 'invert(1)' }}
        ></button>
      </div>
    </div>
  );
};
