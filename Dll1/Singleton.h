#pragma once
template<typename T>
class Singleton
{
	struct object_creator
	{
		object_creator()
		{
			Singleton<T>::instance();
		}
		inline void do_nothing() const  {}
	};

	static object_creator create_object;

public:
	typedef T object_type;
	static T& instance()
	{
		static T obj;
		//���do_nothing��ȷ��create_object���캯��������
		//���ģ��ı����й�
		create_object.do_nothing();
		return obj;
	}

};
template <typename T> typename Singleton<T>::object_creator Singleton<T>::create_object;
