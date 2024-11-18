import { useState } from "react";
import { post } from '../services/httpService.js'

const useAuth = () => {
    const [error, setError] = useState(null);

    const authenticate = async ({user, pass}) => {
        try {
            await post('/auth/login', {username: user, password: pass});
        }
        catch(err){
            setError(err);
        }
    }

    return { authenticate, error };
}

export default useAuth;