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
        const normalizedUser = {
            ...userData,
            role: userData.role || (Array.isArray(userData.roles) ? userData.roles[0] : userData.roles) || 'Customer'
        };
        localStorage.setItem('accessToken', userData.accessToken);
        localStorage.setItem('user', JSON.stringify(normalizedUser));
        setUser(normalizedUser);
    };

    const logout = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('user');
        setUser(null);
    };

    const updateUser = (updatedFields) => {
        if (!user) return;
        const updatedUser = { ...user, ...updatedFields };
        localStorage.setItem('user', JSON.stringify(updatedUser));
        setUser(updatedUser);
    };

    return (
        <AuthContext.Provider value={{ user, loading, loginUser, logout, updateUser, isAdmin: user?.role === 'Admin' }}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuth = () => useContext(AuthContext);
