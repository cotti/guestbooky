import { useState, useEffect } from "react";
import { get, del } from '../services/httpService.js'

const useMessages = () => {
    const [messages, setMessages] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [totalMessages, setTotalMessages] = useState(0);
    const [page, setPage] = useState(1);
    const amountPerPage = 10;

    useEffect(() => {
        const fetchMessages = async () => {
            try{
                setLoading(true);
                const data = await get('/message', {
                    headers: {
                        'Range': 'messages=' + ((page - 1) * amountPerPage) + '-' + (((page - 1) * amountPerPage) + amountPerPage),
                    },
                });

                const [, rangeInfo] = data.headers['content-range'].split(' ');
                const [, total] = rangeInfo.split('/');

                setTotalMessages(parseInt(total));
                setMessages(data.data);
            } catch(err) {
                setError(err);
            } finally {
                setLoading(false);
            }
        };
        fetchMessages();
    }, [page]);

    const removeMessage = async (id) => {
        try{
            await del('/message', {id: id});
            setMessages((prevlist) => prevlist.filter(message => message.id !== id));
            setTotalMessages(totalMessages - 1)
        }catch(err){
            setError(err);
        }
    };

    const nextPage = () => {
        if(page * amountPerPage < totalMessages)
            setPage((page + 1));
    }
    const previousPage = () => {
        if (page > 1)
            setPage((page - 1));
    }

    return {
        messages,
        totalMessages,
        loading,
        error,
        removeMessage,
        page,
        nextPage,
        previousPage
    }
}

export default useMessages;