import React, { useRef } from 'react';
import useAuth from '../hooks/useAuth.js';

import './LoginPage.css'

const LoginPage = ({onLoggedIn}) =>
{
    const userInputRef = useRef(null);
    const passInputRef = useRef(null);
    const { authenticate, error } = useAuth();

    const handleSubmit = (e) => {
        e.preventDefault()
        const user = userInputRef.current.value;
        const pass = passInputRef.current.value;

        authenticate({user, pass})
            .then(() => {
                if (error === null) {
                    onLoggedIn(true)
                }
        });
    }

    return(
        <>
        <div className="login-form fade-in">
            <form action="post" onSubmit={handleSubmit}>
                <label>User: </label><input type='text' name='username' placeholder=''
                                                 ref={userInputRef}></input>
                <label>Password: </label><input type='password' name='password' placeholder=''
                                                ref={passInputRef}></input>
                <button type='submit' className='login-button'>Login</button>
            </form>
        </div>
        </>
    )
}

export default LoginPage;