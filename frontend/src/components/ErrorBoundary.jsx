import React from 'react';

class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error) {
        return { hasError: true, error };
    }

    componentDidCatch(error, errorInfo) {
        console.error("ErrorBoundary caught an error:", error, errorInfo);
    }

    render() {
        if (this.state.hasError) {
            return (
                <div className="page container" style={{ textAlign: 'center', paddingTop: '100px' }}>
                    <div className="card" style={{ padding: '40px', maxWidth: '500px', margin: '0 auto' }}>
                        <h2 style={{ color: 'var(--danger)', marginBottom: '16px' }}>Something went wrong.</h2>
                        <p style={{ color: 'var(--text-secondary)', marginBottom: '24px' }}>
                            The application crashed in this section. We've logged the error.
                        </p>
                        <button
                            className="btn btn-primary"
                            onClick={() => window.location.reload()}
                        >
                            Reload Page
                        </button>
                        {process.env.NODE_ENV === 'development' && (
                            <pre style={{ marginTop: '20px', textAlign: 'left', fontSize: '0.8rem', background: '#000', padding: '10px', overflow: 'auto' }}>
                                {this.state.error?.toString()}
                            </pre>
                        )}
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}

export default ErrorBoundary;
