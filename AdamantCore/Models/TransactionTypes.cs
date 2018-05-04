namespace Adamant.Models
{
	public enum TransactionType: int
	{
		Send,
		Signature,
		Delegate,
		Vote,
		Multi,
		Dapp,
		InTransfer,
		OutTransfer,
		ChatMessage,
	}

	public enum ChatType: int
	{
		MessageOld,
		Message
	}
}
