import { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const stored = localStorage.getItem('user');
        if (stored) {
            try { setUser(JSON.parse(stored)); } catch (e) { localStorage.removeItem('user'); }
        }
        setLoading(false);
    }, []);

    const loginUser = (userData) => {
        localStorage.setItem('accessToken', userData.accessToken);
        localStorage.setItem('user', JSON.stringify(userData));
        setUser(userData);
    };

    const logout = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('user');
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, loading, loginUser, logout, isAdmin: user?.role === 'Admin' }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => useContext(AuthContext);
