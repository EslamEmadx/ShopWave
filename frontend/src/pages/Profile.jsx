import { useState, useEffect } from 'react';
import { getProfile, updateProfile } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { FiUser, FiSave } from 'react-icons/fi';
import toast from 'react-hot-toast';

export default function Profile() {
    const { user } = useAuth();
    const [profile, setProfile] = useState(null);
    const [form, setForm] = useState({ username: '', phone: '', address: '', city: '' });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getProfile().then(r => {
            setProfile(r.data);
            setForm({ username: r.data.username || '', phone: r.data.phone || '', address: r.data.address || '', city: r.data.city || '' });
        }).catch(() => { }).finally(() => setLoading(false));
    }, []);

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            await updateProfile(form);
            toast.success('Profile updated!');
        } catch (e) { toast.error('Failed to update'); }
    };

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;

    return (
        <div className="page container">
            <div className="page-header"><h1 className="page-title">My Profile</h1></div>
            <div style={{ maxWidth: 600 }}>
                <div className="card" style={{ padding: 32 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 32, paddingBottom: 24, borderBottom: '1px solid var(--border-color)' }}>
                        <div style={{ width: 64, height: 64, borderRadius: '50%', background: 'var(--accent-gradient)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.5rem', fontWeight: 800, color: 'white' }}>
                            {user?.username?.charAt(0).toUpperCase()}
                        </div>
                        <div>
                            <div style={{ fontWeight: 700, fontSize: '1.2rem' }}>{profile?.username}</div>
                            <div style={{ color: 'var(--text-secondary)' }}>{profile?.email}</div>
                            <div style={{ fontSize: '0.8rem', color: 'var(--accent-primary)', marginTop: 4 }}>{profile?.role}</div>
                        </div>
                    </div>

                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label className="form-label">Username</label>
                            <input className="form-input" value={form.username} onChange={e => setForm({ ...form, username: e.target.value })} />
                        </div>
                        <div className="form-group">
                            <label className="form-label">Phone</label>
                            <input className="form-input" placeholder="Your phone number" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
                        </div>
                        <div className="form-group">
                            <label className="form-label">Address</label>
                            <input className="form-input" placeholder="Your address" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
                        </div>
                        <div className="form-group">
                            <label className="form-label">City</label>
                            <input className="form-input" placeholder="Your city" value={form.city} onChange={e => setForm({ ...form, city: e.target.value })} />
                        </div>
                        <button type="submit" className="btn btn-primary btn-lg"><FiSave /> Save Changes</button>
                    </form>
                </div>
            </div>
        </div>
    );
}
