import {useState} from 'react'
import AdminPage from './components/AdminPage'
import LoginPage from './components/LoginPage'

import './App.css'

function App() {
    const [isLoggedIn, setIsLoggedIn] = useState(false)

    const handleLoggedIn = (newValue) => {
        setIsLoggedIn(newValue)
    }
    return (
        <>
          <div className="header"><h1>Guestbooky Admin</h1></div>
            <div className="content">
                {isLoggedIn ? <AdminPage className="content"/> :
                    <LoginPage onLoggedIn={handleLoggedIn} className="content"/>}
            </div>
        </>
    )
}

export default App
