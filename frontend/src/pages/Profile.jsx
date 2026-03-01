import { useState, useEffect } from 'react';
import { getProfile, updateProfile, uploadAvatar, deleteAvatar, resolveAvatarUrl } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { FiUser, FiSave, FiCamera, FiTrash2 } from 'react-icons/fi';
import toast from 'react-hot-toast';

export default function Profile() {
    const { user, updateUser } = useAuth();
    const [profile, setProfile] = useState(null);
    const [form, setForm] = useState({ username: '', phone: '', address: '', city: '' });
    const [loading, setLoading] = useState(true);
    const [imgError, setImgError] = useState(false);

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
            updateUser({ username: form.username });
            toast.success('Profile updated!');
        } catch (e) { toast.error('Failed to update'); }
    };

    const handleAvatarUpload = async (e) => {
        let file = e.target.files?.[0];
        if (!file) return;

        const MAX_SIZE = 1 * 1024 * 1024; // 1MB
        if (file.size > MAX_SIZE) {
            try {
                const image = new Image();
                image.src = URL.createObjectURL(file);
                await new Promise(resolve => { image.onload = resolve; });

                const canvas = document.createElement('canvas');
                let { width, height } = image;
                const maxDim = 768;

                if (width > height && width > maxDim) {
                    height *= maxDim / width;
                    width = maxDim;
                } else if (height > maxDim) {
                    width *= maxDim / height;
                    height = maxDim;
                }

                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.drawImage(image, 0, 0, width, height);

                const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/webp', 0.8) || canvas.toBlob(resolve, 'image/jpeg', 0.8));
                if (blob.size > MAX_SIZE) {
                    toast.error('Image still too large after compression.');
                    return;
                }
                file = new File([blob], file.name.replace(/\.[^/.]+$/, "") + ".webp", { type: blob.type });
            } catch (err) {
                toast.error('Failed to process image');
                return;
            }
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            const res = await uploadAvatar(formData);
            const profilePictureUrl = `${res.data.profilePictureUrl}&t=${Date.now()}`;
            setImgError(false);
            updateUser({ profilePictureUrl });
            toast.success('Avatar updated!');
            setProfile(prev => ({ ...prev, profilePictureUrl }));
        } catch (error) {
            toast.error(error.response?.data?.message || 'Failed to upload avatar');
        }
    };

    const handleAvatarDelete = async () => {
        if (!confirm('Remove profile photo?')) return;
        try {
            await deleteAvatar();
            updateUser({ profilePictureUrl: null });
            toast.success('Avatar removed');
            setProfile(prev => ({ ...prev, profilePictureUrl: null }));
        } catch (error) {
            toast.error('Failed to remove avatar');
        }
    };

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;

    return (
        <div className="page container">
            <div className="page-header"><h1 className="page-title">My Profile</h1></div>
            <div style={{ maxWidth: 600 }}>
                <div className="card" style={{ padding: 32 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 32, paddingBottom: 24, borderBottom: '1px solid var(--border-color)' }}>
                        <div style={{ display: 'flex', flexDirection: 'column', gap: 8, alignItems: 'center' }}>
                            <label style={{ position: 'relative', cursor: 'pointer', display: 'inline-block' }}>
                                <input type="file" accept="image/jpeg, image/png, image/webp" style={{ display: 'none' }} onChange={handleAvatarUpload} />
                                {(user?.profilePictureUrl || profile?.profilePictureUrl) && !imgError ? (
                                    <img src={resolveAvatarUrl(user?.profilePictureUrl || profile?.profilePictureUrl)} alt="" style={{ width: 80, height: 80, borderRadius: '50%', objectFit: 'cover', border: '2px solid var(--accent-primary)' }} onError={() => setImgError(true)} />
                                ) : (
                                    <div style={{ width: 80, height: 80, borderRadius: '50%', background: 'var(--accent-gradient)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '2rem', fontWeight: 800, color: 'white' }}>
                                        {user?.username?.charAt(0).toUpperCase()}
                                    </div>
                                )}
                                <div style={{ position: 'absolute', bottom: 0, right: 0, background: 'var(--accent-primary)', color: 'white', width: 32, height: 32, borderRadius: 9999, display: 'flex', alignItems: 'center', justifyContent: 'center', flex: '0 0 auto', border: '2px solid white' }}>
                                    <FiCamera size={14} />
                                </div>
                            </label>
                            {(user?.profilePictureUrl || profile?.profilePictureUrl) && (
                                <button type="button" onClick={handleAvatarDelete} style={{ background: 'none', border: 'none', color: 'var(--danger)', fontSize: '0.8rem', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 4 }}>
                                    <FiTrash2 size={12} /> Remove photo
                                </button>
                            )}
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
